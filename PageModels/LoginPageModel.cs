using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Food_maui.Data;
using Food_maui.Models;
using Food_maui.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel; // For MainThread
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Authentication;
using System.Security.Cryptography;
using System.Text.Json.Serialization; // For WebAuthenticator

namespace Food_maui.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        public const string ProjectQueryKey = "project";
        private readonly IAuthenticationService _authenticationService;
        private readonly IModalErrorHandler _errorHandler;
        private readonly UserMetadataService _userMetadataService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private double _horizontalLength;

        public LoginPageModel(IAuthenticationService authenticationService, IModalErrorHandler errorHandler, UserMetadataService userMetadataService)
        {
            _authenticationService = authenticationService;
            _errorHandler = errorHandler;
            _userMetadataService = userMetadataService;

            if (DeviceDisplay.Current != null)
            {
                double screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
                double elementWidth = 50;
                HorizontalLength = (screenWidth - elementWidth) / 2;
                System.Console.WriteLine($"HorizontalLength: {_userMetadataService}");
            }
            else
            {
                HorizontalLength = 0; // Default value
                System.Console.WriteLine("DeviceDisplay or MainDisplayInfo is null.");
            }

            // Load saved credentials if RememberMe is enabled
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _userMetadataService.LoadRememberMeDataAsync();
                if (_userMetadataService.RememberMe)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Username = _userMetadataService.UserID ?? string.Empty;
                        Password = _userMetadataService.Password ?? string.Empty;
                        RememberMe = true;
                    });
                }
            });
        }

        public LoginPageModel(IAuthenticationService? authenticationService, IModalErrorHandler? errorHandler)
        {
            _authenticationService = authenticationService;
            _errorHandler = errorHandler;
        }

        private async Task Toasty(string message)
        {
            // Display a toast notification using CommunityToolkit.Maui.Alerts
            if (Application.Current?.MainPage != null)
            {
                var toast = Toast.Make(message, ToastDuration.Short, 14);
                await toast.Show();
            }
            else
            {
                System.Console.WriteLine("MainPage is null. Cannot display toast.");
            }
        }

        private async Task HandleErrorSafely(string errorMessage)
        {
            try
            {
                // Attempt to handle the error using the error handler
                if (_errorHandler != null)
                {
                    await _errorHandler.HandleError(new Exception(errorMessage));
                }
                else
                {
                    System.Console.WriteLine("Error handler is null. Cannot handle error.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception and show a fallback error message
                System.Console.WriteLine($"Error while handling error: {ex.Message}");
            }

            // Show the error message to the user
            await Toasty(errorMessage);
        }

        [RelayCommand]
        private async Task SignIn()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    await HandleErrorSafely("Username and password cannot be empty.");
                    return;
                }

                var loginData = new
                {
                    LoginBy = "HD",
                    UserName = Username,
                    Password = Password
                };

                string json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                System.Console.WriteLine($"Content: {await content.ReadAsStringAsync()}");

                try
                {
                    using var client = new HttpClient();
                    var response = await client.PostAsync("http://99.89.32.196/api/Buisness/BuisnessLogin", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<LoginResponse>(responseData, options);

                        if (result?.UsersData != null && result.UsersData.Count > 0)
                        {
                            var userData = result.UsersData.FirstOrDefault();

                            if (userData != null && !string.IsNullOrEmpty(userData.UserID))
                            {
                                // Save all response data to UserMetadataService
                                if (_userMetadataService != null)
                                {
                                    Console.WriteLine("LoginDataSave");
                                    _userMetadataService.UserName = userData.UserName;
                                    _userMetadataService.UserID = userData.UserID;
                                    _userMetadataService.RoleName = userData.RoleName;
                                    _userMetadataService.Restaurant = userData.Restaurant;
                                    _userMetadataService.RestaurantType = userData.RestaurantType;
                                    _userMetadataService.BusinessOwner = userData.BusinessOwner;
                                    _userMetadataService.TaxPercentage = userData.TaxPercentage;
                                    _userMetadataService.Zip = userData.Zip;
                                    _userMetadataService.DeliveryManagedBy = userData.DeliveryManagedBy;
                                    _userMetadataService.HdBusinessRulesChargeType = userData.HdBusinessRulesChargeType;
                                    _userMetadataService.Latitude = userData.Latitude;
                                    _userMetadataService.Longitude = userData.Longitude;
                                    _userMetadataService.Password = Password;

                                    // Save RememberMe state and credentials if enabled
                                    _userMetadataService.RememberMe = RememberMe;
                                    await _userMetadataService.SaveRememberMeDataAsync();

                                    // Save user data persistently
                                    await _userMetadataService.SaveUserDataAsync();
                                }

                                await Toasty("Login successful!");

                                if (Shell.Current != null)
                                {
                                    await Shell.Current.GoToAsync($"//main?userName={userData.UserName}");
                                }
                                else
                                {
                                    System.Console.WriteLine("Shell.Current is null. Cannot navigate.");
                                }
                            }
                            else
                            {
                                await HandleErrorSafely("Invalid user data received.");
                            }
                        }
                        else
                        {
                            await HandleErrorSafely("Invalid username or password.");
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Console.WriteLine($"Error Response: {errorContent}");
                        await HandleErrorSafely("Failed to connect to the server.");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    System.Console.WriteLine($"HttpRequestException: {httpEx.Message}");
                    await HandleErrorSafely("Connection failure. Please check your network and try again.");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Exception: {ex.Message}");
                    await HandleErrorSafely("An unexpected error occurred.");
                }
            }
            catch (Exception ex)
            {
                await HandleErrorSafely("An unexpected error occurred.");
            }
        }

        [RelayCommand]
        private async Task RegisterNewBusiness()
        {
            try
            {
                var url = "http://www.halaldeliveries.com/Pages/BuisnessOwner/BusinessRegistration.aspx";
                await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error opening URL: {ex.Message}");
                await HandleErrorSafely("Unable to open the registration page. Please try again later.");
            }
        }

        [RelayCommand]
        private async Task SignInWithGoogle()
        {
            try
            {
                // Step 1: Perform Google Authentication
                System.Console.WriteLine($"Google Auth Email");
                var clientId = "830184756662-kppto5i0hm839rvp6aihhljsd5p2bhn3.apps.googleusercontent.com";
                var redirect_uri = "com.business.foodmaui:/callback";
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri("https://accounts.google.com/o/oauth2/v2/auth?client_id=830184756662-kppto5i0hm839rvp6aihhljsd5p2bhn3.apps.googleusercontent.com&redirect_uri=com.business.foodmaui:/callback&response_type=code&scope=email%20profile%20openid"),
                    new Uri(redirect_uri));
                // Step 2: Extract the email from the Google metadata
                if (authResult.Properties.TryGetValue("code", out var code))
                {
                    System.Console.WriteLine($"Google Auth Code: {code}");
                    var email = await GetEmailFromAccessToken(code, clientId,  redirect_uri);
                System.Console.WriteLine($"Google Auth Email: {email}");

                    // Step 3: Send the email to the backend
                    var loginData = new
                    {
                        LoginBy = "Google",
                        UserName = email
                    };

                    string json = JsonSerializer.Serialize(loginData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    using var client = new HttpClient();
                    var response = await client.PostAsync("http://99.89.32.196/api/Buisness/BuisnessLogin", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<LoginResponse>(responseData, options);
                        Console.WriteLine("response data: " + responseData);
                        if (result?.UsersData != null && result.UsersData.Count > 0)
                        {
                            var userData = result.UsersData.FirstOrDefault();

                            if (userData != null && !string.IsNullOrEmpty(userData.UserID))
                            {
                                // Save user metadata to the shared service
                                if (_userMetadataService != null)
                                {
                                    _userMetadataService.UserName = userData.UserName;
                                    _userMetadataService.UserID = userData.UserID;
                                    _userMetadataService.RoleName = userData.RoleName;
                                    _userMetadataService.Restaurant = userData.Restaurant;
                                    _userMetadataService.RestaurantType = userData.RestaurantType;
                                    _userMetadataService.BusinessOwner = userData.BusinessOwner;
                                    _userMetadataService.TaxPercentage = userData.TaxPercentage;
                                    _userMetadataService.Zip = userData.Zip;
                                    _userMetadataService.DeliveryManagedBy = userData.DeliveryManagedBy;
                                    _userMetadataService.HdBusinessRulesChargeType = userData.HdBusinessRulesChargeType;
                                    _userMetadataService.Latitude = userData.Latitude;
                                    _userMetadataService.Longitude = userData.Longitude;

                                    // Save user data persistently
                                    await _userMetadataService.SaveUserDataAsync();
                                }
                                else
                                {
                                    System.Console.WriteLine("UserMetadataService is null. Cannot save user metadata.");
                                    await HandleErrorSafely("An internal error occurred. Please try again later.");
                                }

                                await Toasty("Google login successful!");

                                if (Shell.Current != null)
                                {
                                    await Shell.Current.GoToAsync($"//main?userName={userData.UserName}");
                                }
                                else
                                {
                                    System.Console.WriteLine("Shell.Current is null. Cannot navigate.");
                                }
                            }
                            else
                            {
                                await HandleErrorSafely("Invalid user data received.");
                            }
                        }
                        else
                        {
                            await HandleErrorSafely("Google login failed.");
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Console.WriteLine($"Error Response: {errorContent}");
                        await HandleErrorSafely("Failed to connect to the server.");
                    }
                }
                else
                {
                    await HandleErrorSafely("Failed to retrieve email from Google authentication.");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception during Google Auth: {ex.Message}");
                await HandleErrorSafely("An error occurred during Google authentication.");
            }
        }

        private async Task<string> GetEmailFromAccessToken(string authorizationCode, string clientId, string redirectUri)
        {
            try
            {
                // Step 1: Generate a code verifier and code challenge for PKCE
                string codeVerifier = GenerateCodeVerifier();

                // Step 2: Exchange the authorization code for an access token
                var tokenEndpoint = "https://oauth2.googleapis.com/token";
                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    // new KeyValuePair<string, string>("code_verifier", codeVerifier) // PKCE verifier
                });
                using var httpClient = new HttpClient();
                var tokenResponse = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
                tokenResponse.EnsureSuccessStatusCode();

                var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenResponseContent);
                Console.WriteLine($"Token Request: {tokenResponseContent}");

                if (tokenData != null && !string.IsNullOrEmpty(tokenData.AccessToken))
                {
                    // Step 3: Use the access token to get the user's email
                    var accessToken = tokenData.AccessToken;
                    Console.WriteLine($"Access Token: {accessToken}");
                    var userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var userInfoResponse = await httpClient.GetAsync(userInfoEndpoint);
                    userInfoResponse.EnsureSuccessStatusCode();

                    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserInfoResponse>(userInfoContent);
                    Console.WriteLine($"User Info: {userInfoContent}");
                    if (userInfo != null && userInfo.VerifiedEmail)
                    {
                        return userInfo.Email; // Return the user's email
                    }
                    else
                    {
                        throw new Exception("Failed to retrieve user's email.");
                    }
                }
                else
                {
                    throw new Exception("Failed to retrieve access token.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetEmailFromAccessToken: {ex.Message}");
                throw;
            }
        }

        // Generate a code verifier for PKCE
        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
        public async Task<string> AuthenticateWithGoogleAsync()
        {
            var clientId = "830184756662-4pnloirsrd1khe8plu29iijffem7prpl.apps.googleusercontent.com";
            var redirectUri = "com.business.foodmaui:/oauth2redirect";
            var authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            // Step 1: Open the authorization URL
            var authUrl = $"{authorizationEndpoint}?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=email";
            var authResult = await WebAuthenticator.AuthenticateAsync(new Uri(authUrl), new Uri(redirectUri));

            // Step 2: Exchange the authorization code for an access token
            var authCode = authResult.Properties["code"];
            using var httpClient = new HttpClient();
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", authCode),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var tokenResponse = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
            tokenResponse.EnsureSuccessStatusCode();

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            return tokenResponseContent; // This contains the access token and other details
        }

        public class UserInfoResponse
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("verified_email")]
            public bool VerifiedEmail { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("picture")]
            public string Picture { get; set; }

            [JsonPropertyName("locale")]
            public string Locale { get; set; }
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("scope")]
            public string Scope { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; }
        }

        public class LoginResponse
        {
            public List<UserData>? UsersData { get; set; }
            public List<ErrorData>? ErrorData { get; set; } // Add ErrorData property
        }

        public class ErrorData
        {
            public string? ErrorSubject { get; set; }
            public string? ErrorValue { get; set; }
        }

        public class UserData
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
            public string? LoginBy { get; set; }
            public int? LocationID { get; set; }
            public string? UserID { get; set; }
            public string? RestaurantType { get; set; }
            public string? Restaurant { get; set; }
            public string? BusinessOwner { get; set; }
            public string? RoleName { get; set; }
            public string? TaxPercentage { get; set; }
            public string? Zip { get; set; }
            public string? DeliveryManagedBy { get; set; }
            public string? HdBusinessRulesChargeType { get; set; }
            public string? Latitude { get; set; }
            public string? Longitude { get; set; }
        }
    }
}
