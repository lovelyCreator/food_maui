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
using Microsoft.Maui.Authentication; // For WebAuthenticator

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
                System.Console.WriteLine($"HorizontalLength: {HorizontalLength}");
            }
            else
            {
                HorizontalLength = 0; // Default value
                System.Console.WriteLine("DeviceDisplay or MainDisplayInfo is null.");
            }
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
                System.Console.WriteLine($"Content: {await content.ReadAsStringAsync()}"); // Log the actual content

                try
                {
                    using var client = new HttpClient();
                    var response = await client.PostAsync("http://99.89.32.196/api/Buisness/BuisnessLogin", content);

                    System.Console.WriteLine($"Response: {response}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        System.Console.WriteLine($"Response of Login: {responseData}");

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var result = JsonSerializer.Deserialize<LoginResponse>(responseData, options);

                        // Log the entire result object
                        System.Console.WriteLine($"Deserialized Result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

                        if (result?.ErrorData != null && result.ErrorData.Count >= 0)
                        {
                        System.Console.WriteLine("Error");
                            var error = result.ErrorData.FirstOrDefault();
                            var errorMessage = error?.ErrorValue ?? "Invalid username or password.";
                            await HandleErrorSafely(errorMessage);
                            return;
                        }
                        if (result?.UsersData != null && result.UsersData.Count > 0)
                        {
                            // Access the first user data
                            var userData = result.UsersData.FirstOrDefault();

                            // Log the user data for debugging
                            System.Console.WriteLine($"UserName: {userData?.UserName}");
                            System.Console.WriteLine($"UserID: {userData?.UserID}");
                            System.Console.WriteLine($"RoleName: {userData?.RoleName}");

                            if (userData != null && !string.IsNullOrEmpty(userData.UserID))
                            {
                                // Save user metadata to the shared service
                                if (_userMetadataService != null)
                                {
                                    System.Console.WriteLine($"UserID: {userData.UserID}");
                                    // Save user metadata to the shared service
                                    _userMetadataService.UserName = userData.UserName;
                                    _userMetadataService.UserID = userData.UserID;

                                    // Save user data persistently
                                    await _userMetadataService.SaveUserDataAsync();
                                }
                                else
                                {
                                    System.Console.WriteLine("UserMetadataService is null. Cannot save user metadata.");
                                    await HandleErrorSafely("An internal error occurred. Please try again later.");
                                }

                                await Toasty("Login successful!");

                                if (Shell.Current != null)
                                {
                                    // Pass user details to the main page
                                    await Shell.Current.GoToAsync($"//main?userName={userData.UserName}");
                                }
                                else
                                {
                                    System.Console.WriteLine("Shell.Current is null. Cannot navigate.");
                                }
                            }
                            else
                            {
                                var errorMessage = "Invalid user data received.";
                                await HandleErrorSafely(errorMessage);
                            }
                        }
                        else
                        {
                            var errorMessage = "Invalid username or password.";
                            await HandleErrorSafely(errorMessage);
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Console.WriteLine($"Error Response: {errorContent}");
                        var errorMessage = "Failed to connect to the server.";
                        await HandleErrorSafely(errorMessage);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    System.Console.WriteLine($"HttpRequestException: {httpEx.Message}");
                    var errorMessage = "Connection failure. Please check your network and try again.";
                    await HandleErrorSafely(errorMessage);
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
            // Navigate to the registration page
            await Shell.Current.GoToAsync("RegisterPage"); //Adjust the route
        }

        [RelayCommand]
        private async Task SignInWithGoogle()
        {
            try
            {
                    System.Console.WriteLine($"Google Auth Email: ");
                // Step 1: Perform Google Authentication
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri("https://accounts.google.com/o/oauth2/v2/auth?client_id=830184756662-4pnloirsrd1khe8plu29iijffem7prpl.apps.googleusercontent.com&redirect_uri=food_maui://callback&response_type=code&scope=email"),
                    new Uri("food_maui://callback"));

                // Step 2: Extract the email from the Google metadata
                if (authResult.Properties.TryGetValue("email", out var email))
                {
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
                        var result = JsonSerializer.Deserialize<LoginResponse>(responseData);

                        if (result?.UsersData != null && result.UsersData.Count > 0)
                        {
                            await Toasty("Google login successful!");
                            await Shell.Current.GoToAsync($"//main");
                        }
                        else
                        {
                            await HandleErrorSafely("Google login failed.");
                        }
                    }
                    else
                    {
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
