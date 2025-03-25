using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using Food_maui;
using Android.Media;
using System;

namespace LocalNotificationDemo.Platforms.Android
{
    public class NotificationManagerService
    {
        private const int NotificationId = 1000;
        private const int PopupNotificationId = 2000;

        public void ShowNotification(string title, string message)
        {
            var context = MainApplication.Context;

            var intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            // Create a notification style that shows more content (like WhatsApp)
            var bigTextStyle = new NotificationCompat.BigTextStyle()
                .BigText(message)
                .SetBigContentTitle(title)
                .SetSummaryText("New Message");

            // Build a more engaging notification with high priority
            var notificationBuilder = new NotificationCompat.Builder(context, Food_maui.MainActivity.ChannelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Food_maui.Resource.Drawable.notification_icon_background)
                .SetColor(global::Android.Graphics.Color.ParseColor("#25D366")) // WhatsApp green color
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetStyle(bigTextStyle)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetCategory(NotificationCompat.CategoryMessage)
                .SetVisibility(NotificationCompat.VisibilityPublic);

            // Add vibration pattern similar to messaging apps
            long[] vibrationPattern = { 0, 250, 250, 250 };
            notificationBuilder.SetVibrate(vibrationPattern);

            // Add a default sound
            notificationBuilder.SetDefaults(NotificationCompat.DefaultSound);

            var notificationManagerCompat = NotificationManagerCompat.From(context);
            
            // Check for notification permission
            if (CheckNotificationPermission(context))
            {
                notificationManagerCompat.Notify(NotificationId, notificationBuilder.Build());
                Console.WriteLine("Regular notification sent successfully");
            }
            else
            {
                Console.WriteLine("Notification permission not granted");
            }
        }

        // New method for popup notifications like WhatsApp calls
        public void ShowPopupNotification(string title, string message)
        {
            try
            {
                var context = MainApplication.Context;
                Console.WriteLine($"ShowPopupNotification: Starting with title={title}, message={message}");
                
                // Check if channel exists
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var notificationManagerService = (NotificationManager)context.GetSystemService(Context.NotificationService);
                    var channel = notificationManagerService.GetNotificationChannel(Food_maui.MainActivity.HighPriorityChannelId);
                    
                    if (channel == null)
                    {
                        Console.WriteLine("High priority channel doesn't exist! Creating it now...");
                        // Create the channel on the fly if it doesn't exist
                        var highPriorityChannel = new NotificationChannel(
                            Food_maui.MainActivity.HighPriorityChannelId,
                            "Important Notifications",
                            NotificationImportance.High)
                        {
                            Description = "Channel for important popup notifications"
                        };
                        
                        highPriorityChannel.EnableVibration(true);
                        highPriorityChannel.EnableLights(true);
                        
                        var audioAttributes = new AudioAttributes.Builder()
                            .SetUsage(AudioUsageKind.Notification)
                            .SetContentType(AudioContentType.Sonification)
                            .Build();
                            
                        highPriorityChannel.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone), audioAttributes);
                        
                        notificationManagerService.CreateNotificationChannel(highPriorityChannel);
                        Console.WriteLine("High priority channel created on the fly");
                    }
                    else
                    {
                        Console.WriteLine($"High priority channel exists with importance: {channel.Importance}");
                    }
                }

                // Create intent for when notification is tapped
                var intent = new Intent(context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                intent.PutExtra("notification_action", "popup_tapped");
                
                var pendingIntent = PendingIntent.GetActivity(
                    context, 
                    0, 
                    intent, 
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                // Create a full-screen intent (this makes it popup like a call)
                var fullScreenIntent = PendingIntent.GetActivity(
                    context,
                    1,
                    intent,
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                // Build the notification with high priority settings - SIMPLIFIED VERSION
                var notificationBuilder = new NotificationCompat.Builder(context, Food_maui.MainActivity.HighPriorityChannelId)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetSmallIcon(Food_maui.Resource.Drawable.notification_icon_background)
                    .SetColor(global::Android.Graphics.Color.ParseColor("#128C7E"))
                    .SetAutoCancel(false)
                    .SetContentIntent(pendingIntent)
                    .SetPriority(NotificationCompat.PriorityMax)
                    .SetCategory(NotificationCompat.CategoryCall)
                    .SetVisibility(NotificationCompat.VisibilityPublic)
                    .SetFullScreenIntent(fullScreenIntent, true)
                    .SetOngoing(true);

                // Add simple action buttons without icons
                notificationBuilder.AddAction(0, "Accept", pendingIntent);
                notificationBuilder.AddAction(0, "Decline", pendingIntent);

                // Add vibration pattern
                long[] vibrationPattern = { 0, 500, 500, 500, 500, 500 };
                notificationBuilder.SetVibrate(vibrationPattern);

                // Set sound for the notification
                notificationBuilder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone));

                // Show the notification
                var notificationManagerCompat = NotificationManagerCompat.From(context);
                
                // Check for notification permission
                if (CheckNotificationPermission(context))
                {
                    notificationManagerCompat.Notify(PopupNotificationId, notificationBuilder.Build());
                    Console.WriteLine("Popup notification sent successfully");
                }
                else
                {
                    Console.WriteLine("Notification permission not granted");
                    // Try to request permission (though this should be done elsewhere in your app)
                    RequestNotificationPermission(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing popup notification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Fallback to regular notification if popup fails
                ShowNotification($"{title} (Fallback)", $"{message} - Popup notification failed, showing regular notification instead.");
            }
        }

        private bool CheckNotificationPermission(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                // For Android 13+ (API 33+)
                return context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) == 
                    global::Android.Content.PM.Permission.Granted;
            }
            
            // For older Android versions, no specific permission is needed
            return true;
        }
        
        private void RequestNotificationPermission(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                // This is a simplified approach - in a real app, you'd want to handle this properly
                // through the MainActivity or another activity with proper permission handling
                Console.WriteLine("Notification permission is required for Android 13+");
            }
        }

        public void ScheduleRepeatingNotification(string title, string message, int intervalInMinutes)
        {
            var context = MainApplication.Context;

            var alarmIntent = new Intent(context, typeof(NotificationReceiver));
            alarmIntent.SetAction("com.business.foodmaui.NOTIFICATION_ALARM");
            alarmIntent.PutExtra("title", title);
            alarmIntent.PutExtra("message", message);

            var requestCode = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
            
            var pendingIntent = PendingIntent.GetBroadcast(
                context, 
                requestCode, 
                alarmIntent, 
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

            var triggerTime = SystemClock.ElapsedRealtime() + intervalInMinutes * 60 * 1000;
            var repeatInterval = intervalInMinutes * 60 * 1000;

            alarmManager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, triggerTime, repeatInterval, pendingIntent);
        }
    }
}