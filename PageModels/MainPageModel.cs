using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Food_maui.Models;

namespace Food_maui.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;

        [ObservableProperty]
        private List<ProjectTask> _tasks = [];


        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        [ObservableProperty]
        private string _selectedDeliveryStatus = "All";

        public List<string> DeliveryStatuses { get; } = new()
        {
            "All",
            "Pending",
            "PickedUp",
            "Delivered",
            "Canceled",
            "Waiting for the customer Pickup",
            "Pickedup by customer"
        };

        public bool HasCompletedTasks
            => Tasks?.Any(t => t.IsCompleted) ?? false;

        public string TodaysOrdersText
        {
            get
            {
                int pendingCount = Tasks?.Count(t => t.Status == "Pending") ?? 0;
                int inProcessCount = Tasks?.Count(t => t.Status == "InProcess") ?? 0;
                return $"Today's Orders (Pending + InProcess): {pendingCount + inProcessCount}";
            }
        }

        [RelayCommand]
        private void NavigatedTo() =>
            _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() =>
            _isNavigatedTo = false;


        [RelayCommand]
        private async Task OpenWebsite()
        {
            var uri = new Uri("https://www.iSofware.com");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
    }
}