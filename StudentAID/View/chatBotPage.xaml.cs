using Microsoft.Maui.Controls;
using StudentAID.ViewModel;
using StudentAID.Services;
using System.Linq;
using StudentAID.Model;

namespace StudentAID.View;

public partial class chatBotPage : ContentPage
{
    public chatBotPage()
	{
		InitializeComponent();
        var openAIService = new OpenAIService(); // Ensure this is instantiated with necessary configuration
        var speechService = new MicRecognition();
        var viewModel = new ChatBotViewModel(openAIService, speechService);
        this.BindingContext = viewModel;

        MessagingCenter.Subscribe<ChatBotViewModel>(this, "ScrollToLastMessage", (sender) =>
        {
            if (MessagesCollectionView.ItemsSource.Cast<object>().LastOrDefault() is object lastItem)
            {
                MessagesCollectionView.ScrollTo(lastItem, position: ScrollToPosition.End, animate: true);
            }
        });
    }

    private async void OnSwipedLeft(object sender, SwipedEventArgs e)
    {
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
        // Navigate to ChatBotPage
        await Shell.Current.GoToAsync("///libraryFinderPage");
    }

    private async void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
        // Navigate to ChatBotPage
        await Shell.Current.GoToAsync("///homePage");
    }

    async private void OnSendClicked(object sender, EventArgs e)
    {
        var viewModel = this.BindingContext as ChatBotViewModel;
        if (viewModel != null)
        {
            // Assuming SendMessageCommand properly updates the Messages collection
            var messageText = MessageEntry.Text;
            viewModel.SendMessageCommand.Execute(messageText);

            // Ensure UI updates are performed on the main thread
            Device.BeginInvokeOnMainThread(() =>
            {
                if (viewModel.Messages.Any())
                {
                    var lastItem = viewModel.Messages.Last();
                    MessagesCollectionView.ScrollTo(viewModel.Messages.Count - 1, position: ScrollToPosition.End, animate: true);
                }

                MessageEntry.Text = string.Empty; // Clear the message entry
            });
                
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert(
                "Message Required",
                "Please type in your question",
                "OK");
            //MessageEntry.IsEnabled = true;
        }
    }

}