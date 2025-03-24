using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using Food_maui;

namespace LocalNotificationDemo.Platforms.Android
{
    public class NotificationManagerService
    {
        private const int NotificationId = 1000;

        public void ShowNotification(string title, string message)
        {
            var context = MainApplication.Context;

            var intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            var notificationBuilder = new NotificationCompat.Builder(context, MainActivity.ChannelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Food_maui.Resource.Drawable.notification_icon_background)
                .SetColor(global::Android.Graphics.Color.Red)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(NotificationId, notificationBuilder.Build());
        }

        public void ScheduleRepeatingNotification(string title, string message, int intervalInMinutes)
        {
            var context = MainApplication.Context;

            var alarmIntent = new Intent(context, typeof(NotificationReceiver));
            alarmIntent.PutExtra("title", title);
            alarmIntent.PutExtra("message", message);

            var pendingIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

            var triggerTime = SystemClock.ElapsedRealtime() + intervalInMinutes * 60 * 1000; // First trigger after interval
            var repeatInterval = intervalInMinutes * 60 * 1000; // Repeat every interval

            alarmManager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, triggerTime, repeatInterval, pendingIntent);
        }
    }
}