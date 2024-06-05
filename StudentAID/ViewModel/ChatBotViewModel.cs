using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using StudentAID.Model;
using StudentAID.Services;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;

namespace StudentAID.ViewModel
{
    public partial class ChatBotViewModel : ObservableObject
    {
        // Using the MVVM Toolkit [ObservableProperty] attribute to simplify property notification
        [ObservableProperty]
        private ObservableCollection<Message> messages = new();

        [ObservableProperty]
        private bool isMessageEntryEnabled = true;

        [ObservableProperty]
        private string messageText;

        [ObservableProperty]
        private string messageEntryPlaceholder = "Type in your message";

        [ObservableProperty]
        private bool enabledStateSpeechButton = true;

        [ObservableProperty]
        private bool enabledStateSendButton = true;

        [ObservableProperty]
        private bool enabledStateCaptureButton = true;

        [ObservableProperty]
        private string micStatusLabelText;

        [ObservableProperty]
        private Color micStatusLabelTextColor;

        [ObservableProperty]
        private bool isListening;

        // ICommand implementation using RelayCommand from the MVVM Toolkit
        public RelayCommand<string> SendMessageCommand { get; }
        public RelayCommand CaptureAndAskCommand { get; }

        public ICommand OnSwipe { get; set; }

        private readonly OpenAIService _openAIService;
        private readonly MicRecognition _micRecognition;
        private readonly TextExtractionService _ocrService;


        public ChatBotViewModel(OpenAIService openAIService, MicRecognition micRecognition)
        {
            _micRecognition = micRecognition;
            _openAIService = openAIService;
            AddBotGreeting();
            SendMessageCommand = new RelayCommand<string>(async (messageText) => await SendMessage(messageText));
            OnSwipe = new Command<string>(async (route) => await Navigation.swipeNavigation(route));
            _ocrService = new TextExtractionService("azure-computer_vision-url-here", "azure-computer_vision-api_key-here");
            CaptureAndAskCommand = new RelayCommand(async () => await CaptureAndExtractText());


        }

        [RelayCommand]
        private async Task CaptureAndExtractText()
        {
            if (!IsInternetAvailable())
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                await ShowInternetRequiredAlert();
                return;
            }

            EnabledStateCaptureButton = false;  // Disable the capture button to prevent new captures during processing

            try
            {
                // Check and request camera permission
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                }

                if (cameraStatus != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert("Camera Permission", "Camera permission is needed to capture images.", "OK");
                    EnabledStateCaptureButton = true;  // Re-enable the capture button if permission is denied
                    return;
                }

                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    using (var stream = await photo.OpenReadAsync())
                    {
                        // Ensure the stream is at the beginning before sending it to the OCR service
                        if (stream.CanSeek)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                        }

                        var extractedText = await _ocrService.ExtractTextFromImageAsync(stream);
                        if (!string.IsNullOrWhiteSpace(extractedText))
                        {
                            // Treat the message as coming from the user
                            MessageText = extractedText;
                            MessagingCenter.Send(this, "ScrollToLastMessage");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("No Text Found", "No text could be extracted from the image.", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during capture and text extraction: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Error capturing or processing the image.", "OK");
            }
            finally
            {
                EnabledStateCaptureButton = true;  // Re-enable the capture button after processing is complete
            }
        }




        private async Task SendMessage(string messageText)
        {
            if (!IsInternetAvailable())
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                await ShowInternetRequiredAlert();
                return;
            }

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                MessageEntryPlaceholder = "Typing...";
                IsMessageEntryEnabled = false;
                EnabledStateSendButton = false;
                EnabledStateCaptureButton = false;
                // Add user message to the conversation history
                var userMessage = new Message { Text = messageText, IsReceived = false };
                Messages.Add(userMessage);

                // Pass the entire conversation history to the OpenAI service
                var botResponse = await _openAIService.GetResponseAsync(messageText, Messages.ToList());

                // Add bot response to the conversation history
                var botMessage = new Message { Text = botResponse, IsReceived = true };
                Messages.Add(botMessage);
                Vibration.Vibrate(TimeSpan.FromMilliseconds(100));

                MessagingCenter.Send(this, "ScrollToLastMessage");
                MessageEntryPlaceholder = "Type in your message";
                IsMessageEntryEnabled = true;
                EnabledStateSendButton = true;
                EnabledStateCaptureButton = true;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                "Message required",
                "Please type in your question",
                "OK");
                MessageEntryPlaceholder = "Type in your message";
                IsMessageEntryEnabled = true;

            }
        }


        private void AddBotGreeting()
        {
            // Add the bot's greeting message to the conversation
            Messages.Add(new Message { Text = "Hey there, I am here to hep you with your studies. Feel free to ask me anything related to your studies and I will do my very best to help you!", IsReceived = true });
        }

        public ICommand SpeechDetectCommand => new Command(async () =>
        {
            if (!IsInternetAvailable())
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                await ShowInternetRequiredAlert();
                return;
            }

            bool IsMicAvailable = await MicPremission();
            if (IsMicAvailable)
            {
                IsMessageEntryEnabled = false;
                EnabledStateSendButton = false;
                EnabledStateCaptureButton = false;
                MessageText = await _micRecognition.SpeechText(UpdateMicStatus);
                IsMessageEntryEnabled = true;
                EnabledStateSendButton = true;
                EnabledStateCaptureButton = true;
            }
        });

        private void UpdateMicStatus(bool isMicOn)
        {
            // Update UI based on mic status
            if (isMicOn)
            {
                IsListening = isMicOn;
                IsMessageEntryEnabled = !isMicOn;
                EnabledStateSpeechButton = !isMicOn;
                EnabledStateSendButton = !isMicOn;
                MicStatusLabelText = "Mic is on, start speaking...";
                MicStatusLabelTextColor = Colors.Green;
                EnabledStateSpeechButton = false; // Optionally disable the start button while recording
            }
            else
            {
                IsListening = isMicOn;
                IsMessageEntryEnabled = !isMicOn;
                EnabledStateSpeechButton = !isMicOn;
                EnabledStateSendButton = !isMicOn;
                MicStatusLabelText = "Mic is off";
                MicStatusLabelTextColor = Colors.Red;
                EnabledStateSpeechButton = true;
            }
        }

        public static async Task<bool> MicPremission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status == PermissionStatus.Granted)
            {
                return true; // Permission was granted
            }
            else
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                bool goToSettingsPage = await Application.Current.MainPage.DisplayAlert(
                       "Microphone Permission Required",
                       "Microphone permission is needed to use this feature. Please go to Settings and enable the microphone permission for this app.",
                       "Go to Settings",
                       "Cancel");

                if (goToSettingsPage)
                {
                    AppInfo.ShowSettingsUI();
                }
                return false; // Permission was not granted
            }
        }

        private bool IsInternetAvailable()
        {
            var current = Connectivity.Current;
            if (current.NetworkAccess == NetworkAccess.Internet)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task ShowInternetRequiredAlert()
        {
            bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                "Internet Required",
                "To use this feature you need to have an active internet connection. Check your Wi-Fi or data settings?",
                "Go to Settings",
                "Cancel");

            if (goToSettings)
            {
                AppInfo.ShowSettingsUI();
            }
        }
    }
}
