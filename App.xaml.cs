using Microsoft.Maui.Controls;

namespace Food_maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        public static IAuthenticationService? AuthenticationService { get; internal set; }
        public static IModalErrorHandler? ErrorHandler { get; internal set; }
        public static UserMetadataService UserMetadataService { get; internal set; }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}