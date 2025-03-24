namespace Food_maui.Services
{
    public class UserMetadataService
    {
        public string? UserName { get; set; }
        public string? UserID { get; set; }

        public UserMetadataService()
        {
            // Initialize UserName and UserID with empty strings
            UserName = "user";
            UserID = "id.com";
        }

        public async Task SaveUserDataAsync()
        {
            // Example: Save data to SecureStorage
            if (UserName != null)
                await SecureStorage.SetAsync("UserName", UserName);
            if (UserID != null)
                await SecureStorage.SetAsync("UserID", UserID);
        }

        public async Task LoadUserDataAsync()
        {
            // Example: Load data from SecureStorage
            UserName = await SecureStorage.GetAsync("UserName") ?? string.Empty;
            UserID = await SecureStorage.GetAsync("UserID") ?? string.Empty;
        }
    }
}