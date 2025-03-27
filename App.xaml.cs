using Microsoft.Maui.Controls;
using Food_maui.Converters;

namespace Food_maui
{
    public partial class App : Application
    {
        public App()
        {
                InitializeComponent();
        }

        public static IAuthenticationService? AuthenticationService { get; internal set; } = null;
        public static IModalErrorHandler? ErrorHandler { get; internal set; } = null;
        public static UserMetadataService UserMetadataService { get; internal set; } = new UserMetadataService();

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                return new Window(new AppShell());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating window: {ex}");
                throw;
            }
        }
    }
}