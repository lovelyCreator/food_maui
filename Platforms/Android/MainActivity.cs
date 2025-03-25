// using Android.App;
// using Android.Content.PM;
// using Android.OS;
// using Microsoft.Maui.Authentication;
// using Android.Content;

// namespace Food_maui
// {
//     [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
//     public class MainActivity : MauiAppCompatActivity
//     {
//     public const string ChannelId = "default_notification_channel";

//     protected override void OnCreate(Bundle savedInstanceState)
//     {
//         base.OnCreate(savedInstanceState);

//         CreateNotificationChannel();
//     }

//     private void CreateNotificationChannel()
//     {
//         if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
//         {
//             var channelName = "Default Notifications";
//             var channelDescription = "Channel for default notifications";
//             var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.Default)
//             {
//                 Description = channelDescription
//             };

//             var notificationManager = (NotificationManager)GetSystemService(NotificationService);
//             notificationManager.CreateNotificationChannel(channel);
//         }
//     }
//     }
// }

using Android.App;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using System;
using System.Linq;

namespace Food_maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public const string ChannelId = "default_notification_channel";
        public const string HighPriorityChannelId = "high_priority_channel"; // New channel for popup notifications

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Console.WriteLine("MainActivity OnCreate called");
            
            // Force recreate notification channels to ensure they're set up correctly
            DeleteAndRecreateNotificationChannels();
            
            // Check if we're coming from a notification
            if (Intent?.HasExtra("notification_action") == true)
            {
                string action = Intent.GetStringExtra("notification_action");
                Console.WriteLine($"Notification action received: {action}");
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                // Check if we already have the permission
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    // Request the permission
                    ActivityCompat.RequestPermissions(this, new[] { Android.Manifest.Permission.PostNotifications }, 100);
                }
            }
        }

        private void DeleteAndRecreateNotificationChannels()
        {
            try
            {
                Console.WriteLine("Recreating notification channels");
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                    
                    // Delete existing channels first
                    try
                    {
                        Console.WriteLine("Checking for existing channels...");
                        var existingChannels = notificationManager.NotificationChannels;
                        foreach (var channel in existingChannels)
                        {
                            Console.WriteLine($"Found existing channel: {channel.Id}, Importance: {channel.Importance}");
                            if (channel.Id == HighPriorityChannelId || channel.Id == ChannelId)
                            {
                                Console.WriteLine($"Deleting channel: {channel.Id}");
                                notificationManager.DeleteNotificationChannel(channel.Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking existing channels: {ex.Message}");
                    }
                    
                    // Create default channel
                    Console.WriteLine("Creating default channel");
                    var defaultChannel = new NotificationChannel(
                        ChannelId,
                        "Default Notifications", 
                        NotificationImportance.Default)
                    {
                        Description = "Channel for default notifications"
                    };
                    
                    // Create high priority channel
                    Console.WriteLine("Creating high priority channel");
                    var highPriorityChannel = new NotificationChannel(
                        HighPriorityChannelId,
                        "Important Notifications",
                        NotificationImportance.High)
                    {
                        Description = "Channel for important popup notifications"
                    };
                    
                    // Configure high priority channel
                    highPriorityChannel.SetShowBadge(true);
                    highPriorityChannel.LockscreenVisibility = NotificationVisibility.Public;
                    highPriorityChannel.EnableVibration(true);
                    highPriorityChannel.EnableLights(true);
                    
                    // Set vibration pattern
                    long[] vibrationPattern = { 0, 500, 500, 500 };
                    highPriorityChannel.SetVibrationPattern(vibrationPattern);
                    
                    // Set sound
                    var audioAttributes = new AudioAttributes.Builder()
                        .SetUsage(AudioUsageKind.Notification)
                        .SetContentType(AudioContentType.Sonification)
                        .Build();
                        
                    highPriorityChannel.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone), audioAttributes);
                    
                    // Create the channels
                    Console.WriteLine("Registering channels with system");
                    notificationManager.CreateNotificationChannel(defaultChannel);
                    notificationManager.CreateNotificationChannel(highPriorityChannel);
                    
                    // Verify channels were created
                    Console.WriteLine("Verifying channels were created");
                    var channels = notificationManager.NotificationChannels;
                    bool foundHighPriority = false;
                    bool foundDefault = false;
                    
                    foreach (var channel in channels)
                    {
                        Console.WriteLine($"Channel exists: {channel.Id}, Name: {channel.Name}, Importance: {channel.Importance}");
                        if (channel.Id == HighPriorityChannelId)
                        {
                            foundHighPriority = true;
                            Console.WriteLine($"High priority channel found with importance: {channel.Importance}");
                        }
                        if (channel.Id == ChannelId)
                        {
                            foundDefault = true;
                        }
                    }
                    
                    if (!foundHighPriority)
                    {
                        Console.WriteLine("WARNING: High priority channel was not created successfully!");
                    }
                    if (!foundDefault)
                    {
                        Console.WriteLine("WARNING: Default channel was not created successfully!");
                    }
                }
                else
                {
                    Console.WriteLine($"Android version too low for notification channels: {Build.VERSION.SdkInt}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating notification channels: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}