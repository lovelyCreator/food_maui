namespace Food_maui.Pages
{
    using Microsoft.Maui.Storage; // Add this for Preferences

    public partial class LoginPage : ContentPage
    {
        public static double HorizontalLength { get; set; }
        public LoginPage()
        {
            InitializeComponent();

            // Set BindingContext
            BindingContext = new PageModels.LoginPageModel(App.AuthenticationService, App.ErrorHandler, App.UserMetadataService);
        }

        private async Task OnLoginSuccessful(UserMetadataService userMetadata)
        {
            // Update the UserMetadataService with the latest data
            App.UserMetadataService.Restaurant = userMetadata.Restaurant;
            App.UserMetadataService.LocationID = userMetadata.LocationID;
            // Update other properties as needed...

            // Navigate to MainPage
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
