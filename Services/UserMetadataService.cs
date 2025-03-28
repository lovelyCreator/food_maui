using System.Text.Json;

namespace Food_maui.Services
{
    public class UserMetadataService
    {
        public string? UserName { get; set; }
        public int? LocationID { get; set; }
        public string? UserID { get; set; }
        public string? RoleName { get; set; }
        public string? Restaurant { get; set; }
        public string? RestaurantType { get; set; }
        public string? BusinessOwner { get; set; }
        public string? TaxPercentage { get; set; }
        public string? Zip { get; set; }
        public string? DeliveryManagedBy { get; set; }
        public string? HdBusinessRulesChargeType { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool RememberMe { get; set; }
        public string? Password { get; set; }
        public DateTime? LastLoginTime { get; set; }

        public UserMetadataService()
        {
            // Initialize properties with default values
            UserName = "User";
            UserID = "id";
            LocationID = 1;
            RoleName = "role";
            Restaurant = "restaurant";
            RestaurantType = "Halal";
            BusinessOwner = "BusinessOwner";
            TaxPercentage = "1";
            Zip = "123";
            DeliveryManagedBy = "TP_DD";
            HdBusinessRulesChargeType = "RevenueShare";
            Latitude = "123";
            Longitude = "123";
            Password = "password";
            RememberMe = false;
        }

        // Method to save user data persistently
        public async Task SaveUserDataAsync()
        {
            // Implement persistent storage logic here (e.g., save to file, database, or secure storage)
            await Task.CompletedTask;
        }

        // Method to load user data persistently
        public async Task LoadUserDataAsync()
        {
            // Implement logic to load user data from persistent storage
            await Task.CompletedTask;
        }

        public async Task SaveRememberMeDataAsync()
        {
            try
            {
                var data = new RememberMeData
                {
                    UserID = UserID,
                    Password = Password,
                    RememberMe = RememberMe,
                    LastLoginTime = DateTime.UtcNow // Save the current UTC time
                };
                string json = JsonSerializer.Serialize(data);
                await SecureStorage.SetAsync("UserMetadata", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving RememberMe data: {ex.Message}");
            }
        }

        public async Task<bool> LoadRememberMeDataAsync()
        {
            try
            {
                string json = await SecureStorage.GetAsync("UserMetadata");
                if (!string.IsNullOrEmpty(json))
                {
                    var data = JsonSerializer.Deserialize<RememberMeData>(json);
                    if (data != null)
                    {
                        UserID = data.UserID;
                        Password = data.Password;
                        RememberMe = data.RememberMe;
                        LastLoginTime = data.LastLoginTime;

                        // Check if the session is still valid (e.g., within 5 hours)
                        if (LastLoginTime.HasValue && (DateTime.UtcNow - LastLoginTime.Value).TotalHours <= 5)
                        {
                            return true; // Session is valid
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading RememberMe data: {ex.Message}");
            }
            return false; // Session is invalid or data is missing
        }

        private class RememberMeData
        {
            public string? UserID { get; set; }
            public string? Password { get; set; }
            public bool RememberMe { get; set; }
            public DateTime? LastLoginTime { get; set; } // Add timestamp for session validation
        }
    }
}