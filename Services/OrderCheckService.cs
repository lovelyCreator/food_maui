using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json.Linq;
using Microsoft.Maui.Controls;
#if ANDROID
using LocalNotificationDemo.Platforms.Android;
#endif
using System.Text.RegularExpressions;

namespace Food_maui.Services
{
    public class OrderCheckService
    {
        private System.Timers.Timer _timer;
        private int _noOrderCount = 0;

        public OrderCheckService()
        {
            StartOrderCheckTimer();
        }

        private void StartOrderCheckTimer()
        {
            _timer = new System.Timers.Timer(60000); // 1 minute interval
            _timer.Elapsed += async (sender, e) => await CheckNewOrders();
            _timer.Start();
        }

        private async Task CheckNewOrders()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var parameters = new { LocationID = "5", deliveryManagedBy = "HD" };
                    var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://99.89.32.196/api/Buisness/NewBusinessOrderNotification", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return;
                    }

                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseString);

                    var newOrder = jsonResponse["newBusinessOrder"][0]["notificationMsg"].ToString();
                    if (newOrder != "0")
                    {
                        var notificationSubject = jsonResponse["newBusinessOrder"][0]["notificationSubject"].ToString();
                        var notificationMsg = jsonResponse["newBusinessOrder"][0]["notificationMsg"].ToString();
                        var plainTextMsg = Regex.Replace(notificationMsg, "<.*?>", string.Empty);

                        Console.WriteLine("New order detected.");
                        _noOrderCount = 0;
                        ShowPopupNotification(notificationSubject, plainTextMsg);
                    }
                    else
                    {
                        _noOrderCount++;
                        if (_noOrderCount % 4 == 0)
                        {
                            var notificationSubject = jsonResponse["newBusinessOrder"][0]["notificationSubject"].ToString();
                            Console.WriteLine("No new order detected.");
                            ShowPopupNotification(notificationSubject, "No new order detected.");
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private void ShowPopupNotification(string title, string message)
        {
#if ANDROID
            var notificationService = new NotificationManagerService();
            notificationService.ShowPopupNotification(title, message);
#endif
        }

        public async Task AcceptOrder(int orderId)
        {
            try
            {
                Console.WriteLine("Accept Orders---------------------------------->");
                using (var client = new HttpClient())
                {
                    var parameters = new { OrderID = orderId, Status = "Accepted" };
                    var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://99.89.32.196/api/Buisness/AcceptOrder", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Order {orderId} accepted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to accept order {orderId}. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while accepting order {orderId}: {ex.Message}");
            }
        }
    }
}