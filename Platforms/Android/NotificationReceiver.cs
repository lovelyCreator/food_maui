using Android.App;
using Android.Content;

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
            notificationService.ShowNotification(title, message);

            Console.WriteLine($"NotificationReceiver: Received broadcast with title: {title} and message: {message}");
        }
    }
}