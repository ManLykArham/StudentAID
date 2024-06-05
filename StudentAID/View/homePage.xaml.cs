using Microsoft.Maui.Controls;
using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using StudentAID.ViewModel;

namespace StudentAID.View
{
    public partial class homePage : ContentPage
    {
        public homePage()
        {
            InitializeComponent();
            
            var viewModel = new HomePageViewModel();
            BindingContext = viewModel; 

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                chatBotFrame.WidthRequest = 850;
                libraryFinderFrame.WidthRequest = 850;
            }
            else
            {
                chatBotFrame.WidthRequest = 325;
                libraryFinderFrame.WidthRequest = 325;
            }
        }

        private async void OnChatBotTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            await AnimateTap(frame);
            try
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
                // Navigate to LibraryFinder Page
                await Shell.Current.GoToAsync("///chatBotPage");
            }
            catch (Exception ex)
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                await Shell.Current.DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnLibraryFinderTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            await AnimateTap(frame);
            try
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
                // Navigate to LibraryFinder Page
                await Shell.Current.GoToAsync("///libraryFinderPage");
            }
            catch (Exception ex)
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                await Shell.Current.DisplayAlert("Navigation Error", ex.Message, "OK");
            }

        }

        private async Task AnimateTap(Frame frame)
        {
            // Scale down animation
            await frame.ScaleTo(0.95, 50, Easing.Linear);
            // Scale back to original size
            await frame.ScaleTo(1, 50, Easing.Linear);
        }
    }
}