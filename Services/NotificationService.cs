using System.Collections.ObjectModel;
using System.Net.Http.Json;

public class NotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ObservableCollection<Order> _newOrders;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(1);
    private CancellationTokenSource _cancellationTokenSource;
    private int _noOrderCount = 0;

    public event Action<Order> NewOrderReceived;
    public event Action NoNewOrdersDetected;

    public NotificationService()
    {
        _httpClient = new HttpClient();
        _newOrders = new ObservableCollection<Order>();
    }

    public ObservableCollection<Order> NewOrders => _newOrders;

    public async Task StartPollingAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("Polling for new orders...");
                var response = await _httpClient.PostAsJsonAsync("http://99.89.32.196/api/Buisness/NewBusinessOrderNotification", new
                {
                    LocationID = "5",
                    deliveryManagedBy = "HD"
                });

                Console.WriteLine($"API call status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<NewOrderResponse>();
                    var orders = result?.newBusinessOrder;
                    Console.WriteLine($"New orders detected: {orders?.Count ?? 0}");
                    if (orders != null && orders.Any(o => o.NotificationMsg != "0"))
                    {
                        _noOrderCount = 0;
                        foreach (var order in orders)
                        {
                            if (!_newOrders.Any(o => o.NotificationMsg == order.NotificationMsg))
                            {
                                var newOrder = new Order
                                {
                                    NotificationSubject = order.NotificationSubject,
                                    NotificationMsg = order.NotificationMsg
                                };
                                _newOrders.Add(newOrder);
                                NewOrderReceived?.Invoke(newOrder);
                            }
                        }
                    }
                    else
                    {
                        _noOrderCount++;
                        Console.WriteLine("No new orders detected.");
                        if (_noOrderCount >= 2)
                        {
                            NoNewOrdersDetected?.Invoke();
                            _noOrderCount = 0;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"API call failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during polling: {ex.Message}");
            }

            try
            {
                await Task.Delay(_pollingInterval, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, exit the loop
                break;
            }
        }
    }

    public void StopPolling()
    {
        _cancellationTokenSource?.Cancel();
    }

    public void AcceptOrder(Order order)
    {
        _newOrders.Remove(order);
        Console.WriteLine($"Order accepted: {order.NotificationMsg}");
        _httpClient.PostAsJsonAsync("http://99.89.32.196/api/Buisness/NewBusinessOrderNotification", order);
    }
}

public class Order
{
    public string NotificationSubject { get; set; }
    public string NotificationMsg { get; set; }
}

public class NewOrderResponse
{
    public List<Order> newBusinessOrder { get; set; }
}