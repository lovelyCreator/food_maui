namespace Food_maui.Pages
{
    public partial class LoginPage : ContentPage
    {
        public static double HorizontalLength { get; set; }
        public LoginPage()
        {
            InitializeComponent();

            // Calculate HorizontalLength
            if (DeviceDisplay.Current != null)
            {
                double screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
                double elementWidth = 50;
                HorizontalLength = (screenWidth - elementWidth) / 2;
                System.Console.WriteLine($"HorizontalLength: {HorizontalLength}");
            }
            else
            {
                HorizontalLength = 0; // Default value
                System.Console.WriteLine("DeviceDisplay is null.");
            }

            // Set BindingContext
            BindingContext = new PageModels.LoginPageModel(App.AuthenticationService, App.ErrorHandler);
        }
    }
}