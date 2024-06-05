using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using StudentAID.ViewModel;
using System.Collections.Specialized; // For observing collection changes
using System.ComponentModel;
using StudentAID.Model;

namespace StudentAID.View;

public partial class libraryFinderPage : ContentPage
{
    LibraryFinderViewModel viewModel;

    public libraryFinderPage()
    {
        InitializeComponent();
        if (Device.Idiom == TargetIdiom.Tablet)
        {
            mapView.Style = (Style)Application.Current.Resources["MapStyleTablet"];
        }
        else
        {
            mapView.Style = (Style)Application.Current.Resources["MapStylePhone"];
        }
        mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(0, 0), // Center of the Earth
                Distance.FromKilometers(10000)));
        viewModel = new LibraryFinderViewModel();
        BindingContext = viewModel;
        viewModel.Libraries.CollectionChanged += Libraries_CollectionChanged;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;

        MessagingCenter.Subscribe<LibraryFinderViewModel>(this, "ApiRequestFailed", (sender) =>
        {
            Dispatcher.Dispatch(() =>
            {
                DisplayAlert("Error", "The API request has failed. Please try again later.", "OK");
            });
        });
    }

    private async void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
        // Navigate to ChatBotPage
        await Shell.Current.GoToAsync("///chatBotPage");
    }

    protected override void OnDisappearing()
    {
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
        base.OnDisappearing();
        // Unsubscribe to avoid memory leaks
        MessagingCenter.Unsubscribe<LibraryFinderViewModel>(this, "ApiRequestFailed");
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LibraryFinderViewModel.CurrentLocation))
        {
            UpdateMapLocation(viewModel.CurrentLocation);
        }
    }

    private void Libraries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // When the Libraries collection changes, update the map pins
        UpdateLibraryPins();
    }

    private void UpdateMapLocation(Location newLocation)
    {
        var mapSpan = MapSpan.FromCenterAndRadius(newLocation, Distance.FromKilometers(1));
        mapView.MoveToRegion(mapSpan);

        // Clear and add the current location pin
        mapView.Pins.Clear();
        mapView.Pins.Add(new Pin
        {
            Label = "Current Location",
            Location = newLocation,
            Type = PinType.Place,
        });

        // Optionally, immediately update library pins as well
        UpdateLibraryPins();
    }

    private void ShowLibrary(Location newLocation)
    {
        var mapSpan = MapSpan.FromCenterAndRadius(newLocation, Distance.FromKilometers(0.15)); // Adjust zoom level here
        mapView.MoveToRegion(mapSpan);
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));

        /*        mapView.Pins.Clear(); // Consider if you want to clear all pins or just select ones
                mapView.Pins.Add(new Pin
                {
                    Label = "Selected Location",
                    Location = newLocation,
                    Type = PinType.Place,
                });

                // Optionally, add back other pins if you cleared them
                UpdateLibraryPins();*/
    }

    private void UpdateLibraryPins()
    {
        // Remove previous library pins, assuming "Current Location" pin is always first
        if (mapView.Pins.Count > 1)
        {
            mapView.Pins.RemoveAt(1); // Adjust based on how you manage pins
        }

        foreach (var library in viewModel.Libraries)
        {
            mapView.Pins.Add(new Pin
            {
                Label = library.LibraryName,
                Location = new Location(library.Latitude, library.Longitude), // Assuming libraries have Latitude and Longitude properties
                Type = PinType.Place,
            });
        }
    }

    /*    private void OnLibrarySelected(object sender, SelectionChangedEventArgs e)
    {
        // Get the newly selected item (assuming single selection mode)
        var selectedLibrary = e.CurrentSelection.FirstOrDefault() as Library;
        if (selectedLibrary != null)
        {
            ShowLibrary(new Location(selectedLibrary.Latitude, selectedLibrary.Longitude));
            // Clear the selection
            ((CollectionView)sender).SelectedItem = null;
        }
    }*/

    private void OnLibraryTapped(object sender, EventArgs e)
    {
        var tap = e as TappedEventArgs;
        if (tap != null)
        {
            var library = tap.Parameter as Library;
            if (library != null)
            {
                ShowLibrary(new Location(library.Latitude, library.Longitude));
                mainScrollView.ScrollToAsync(0, 0, true);
            }
        }
    }



}

