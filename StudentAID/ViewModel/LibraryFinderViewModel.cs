using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentAID.Model;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Maui.Networking;
using System.Linq;
using System.Windows.Input;
using StudentAID.Services;


namespace StudentAID.ViewModel
{
    public partial class LibraryFinderViewModel : ObservableObject, INotifyPropertyChanged
    {
        public ObservableCollection<Library> Libraries { get; } = new ObservableCollection<Library>();

        [ObservableProperty]
        private Location currentLocation;

        [ObservableProperty]
        private bool showUserLocation = false;

        /*[ObservableProperty]
        private string findButtonTextColor = Colors.White.ToString();*/

        [ObservableProperty]
        private Color findButtonBackgroundColor = Color.FromRgb(0, 0, 255);

        [ObservableProperty]
        private string findButtonText = "Find Nearby Libraries";

        public IAsyncRelayCommand GetCurrentLocationCommand { get; }

        public ICommand OnSwipe { get; set; }

        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;

        public LibraryFinderViewModel()
        {
            GetCurrentLocationCommand = new AsyncRelayCommand(GetCurrentLocation);
            OnSwipe = new Command<string>(async (route) => await Navigation.swipeNavigation(route));
        }

        public async Task GetCurrentLocation()
        {
            try
            {
                if (!IsInternetAvailable())
                {
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                    await ShowInternetRequiredAlert();
                    return;
                }
                Libraries.Clear();
                _isCheckingLocation = true;

                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status == PermissionStatus.Granted)
                {
                    // Permission was granted
                    OneMomentState();
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
                    GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    _cancelTokenSource = new CancellationTokenSource();
                    Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                    if (location != null)
                    {
                        FindingLibrariesState();
                        await FindLibrariesAsync(location.Latitude, location.Longitude);
                        CurrentLocation = location;
                        ShowUserLocation = true;
                        Vibration.Vibrate(TimeSpan.FromMilliseconds(500));
                    }
                }
                else if (status == PermissionStatus.Denied && DeviceInfo.Platform != DevicePlatform.iOS)
                {
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                    await ShowLocationRequiredAlert();
                }
            }
            catch (Exception ex)
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _isCheckingLocation = false;
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

        private async Task ShowLocationRequiredAlert()
        {
            bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                "Location Required",
                "To use this feature you need to enable location. Go to settings?",
                "Go to Settings",
                "Cancel");

            if (goToSettings)
            {
                // Open app settings
                AppInfo.ShowSettingsUI();
            }
        }


        public async Task FindLibrariesAsync(double latitude, double longitude)
        {
            try
            {
                var apiKey = "google-api_key-here";
                //var apiKey = "fakeapi";
                var apiUrl = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={latitude},{longitude}&radius=1126.5408&type=library&key={apiKey}";

                using var client = new HttpClient();
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse);

                    if (jsonObject["results"] is JArray results && results.Count > 0)
                    {
                        var temporaryLibraries = new List<Library>();
                        //Libraries.Clear();

                        foreach (var result in results)
                        {
                            temporaryLibraries.Add(new Library
                            {
                                LibraryName = result["name"].ToString(),
                                Latitude = (double)result["geometry"]["location"]["lat"],
                                Longitude = (double)result["geometry"]["location"]["lng"],
                                LibraryDistance = CalculateDistance(latitude, longitude, (double)result["geometry"]["location"]["lat"], (double)result["geometry"]["location"]["lng"])
                            });
                        }
                        Libraries.Clear();
                        foreach (var lib in temporaryLibraries.OrderBy(l => l.LibraryDistance))
                        {
                            Libraries.Add(lib);
                        }
                        LibrariesFoundState();
                    }
                    else
                    {
                        Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                        LibrariesNotFoundState();
                    }
                }
                else
                {
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(1000));
                    MessagingCenter.Send(this, "ApiRequestFailed");
                }
            }
            catch
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
                MessagingCenter.Send(this, "ApiRequestFailed");
            }
        }


        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var baseRad = Math.PI * lat1 / 180;
            var targetRad = Math.PI * lat2 / 180;
            var theta = lon1 - lon2;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return dist;
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


        private void OneMomentState()
        {
            FindButtonText = "One moment please...";
            FindButtonBackgroundColor = Color.FromRgb(180, 180, 180);
        }

        private void FindingLibrariesState()
        {
            FindButtonText = "Finding libraries near you...";
            FindButtonBackgroundColor = Color.FromRgb(180, 180, 180);
        }

        private void LibrariesFoundState()
        {
            FindButtonText = "Libraries Found";
            FindButtonBackgroundColor = Color.FromArgb("#00FF00");
        }

        private void LibrariesNotFoundState()
        {
            FindButtonText = "No Libraries Found";
            FindButtonBackgroundColor = Color.FromArgb("#FF0000");
        }
    }

}