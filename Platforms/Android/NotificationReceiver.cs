using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Food_maui.Services;
using System.Text.RegularExpressions;

namespace LocalNotificationDemo.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("message");

            var notificationService = new NotificationManagerService();
            var notificationManager = NotificationManagerCompat.From(context);

            if (intent.Action == "com.business.foodmaui.ACCEPT_ACTION" || intent.Action == "com.business.foodmaui.DECLINE_ACTION")
            {
                // Cancel the notification
                var notificationId = intent.GetIntExtra("notification_id", -1);
                if (notificationId != -1)
                {
                    notificationManager.Cancel(notificationId);
                    Console.WriteLine($"NotificationReceiver: Canceled notification with ID: {notificationId}");
                }

                if (intent.Action == "com.business.foodmaui.ACCEPT_ACTION")
                {
                    try
                    {
                        // Extract order ID from the message
                        var match = Regex.Match(message, @"Reference # (\d+)");
                        if (match.Success) {
                            int orderId = int.Parse(match.Groups[1].Value);
                            var orderCheckService = new OrderCheckService();
                            orderCheckService.AcceptOrder(orderId).Wait();
                        } else {
                            Console.WriteLine("No Order Number!!!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing accept action: {ex.Message}");
                    }
                }

                // Do not reschedule the popup notification
                Console.WriteLine($"NotificationReceiver: Action {intent.Action} received, no rescheduling.");
            }
            else
            {
                notificationService.ShowNotification(title, message);
                Console.WriteLine($"NotificationReceiver: Received broadcast with title: {title} and message: {message}");
            }
        }
    }
}