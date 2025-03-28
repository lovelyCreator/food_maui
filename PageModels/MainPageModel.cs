using System.ComponentModel;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Food_maui.Models;
using Food_maui.Services;
#if ANDROID
using LocalNotificationDemo.Platforms.Android;
#endif

namespace Food_maui.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        // private readonly NotificationService _notificationService;

        // public MainPageModel(NotificationService notificationService)
        // {
        //     _notificationService = notificationService;
        // }

        private bool _isNavigatedTo;
        private bool _dataLoaded;
        private string _currentStatus = "All";

        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        [ObservableProperty]
        private string _selectedDeliveryStatus = "Pending";

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private bool _isDataFound;

        [ObservableProperty]
        private bool _isNoDataFound;

        [ObservableProperty]
        private List<OrderData> _orders;

        [ObservableProperty]
        private List<EditData> _editDatas;

        [ObservableProperty]
        private bool _isPictureVisible;

        [ObservableProperty]
        private int _totalNumber;
        
        [ObservableProperty]
        private int _newNumber;
        
        [ObservableProperty]
        private int _inProcessNumber;
        
        [ObservableProperty]
        private int _readyNumber;
        
        [ObservableProperty]
        private int _pickedUpNumber;
        
        [ObservableProperty]
        private int _canceledNumber;
        
        [ObservableProperty]
        private int _scheduledNumber;

        [ObservableProperty]
        private List<OrderData> _filteredOrders;

        [ObservableProperty]
        private bool _isLoading;
        
        
        public MainPageModel()
        {
            IsRestaurantant = false;
        }
        private bool _isRestaurantant;

        public bool IsRestaurantant
        {
            get => _isRestaurantant;
            set
            {
                if (_isRestaurantant != value)
                {
                    _isRestaurantant = value;
                    OnPropertyChanged(nameof(IsRestaurantant)); // Notify the UI about the change
                }
            }
        }

        [ObservableProperty]
        private bool _isModalVisible;

        [ObservableProperty]
        private OrderData _selectedOrder;

        [ObservableProperty]
        private EditDataResponse _initialEditDatas;

        [ObservableProperty]
        private bool _isEditDataFounded;

        [ObservableProperty]
        private bool _isRestaurant;

        [ObservableProperty]
        private EditDataResponse _salesEditDatas;
        private OrderData _initialOrder;

        // Define and initialize the test_orders list
        public List<OrderData> TestOrders { get; } = new()
        {
            new OrderData
            {
                locationID = 5,
                restaurant = "Halal Meat And Grill",
                tP_Status = "cancelled",
                pickUpBy = "",
                pickUpByCustomerStatus = false,
                driverName = "",
                salesOrderID = 2598,
                invoiceAmount = 55.96m,
                customerName = "HD Customer",
                delivery_Status = "Pending",
                dateOrder = "2024-04-25T11:26:33.04",
                orderItems = "Behari Beef Boti - 4.00 (count)",
                customerID = 1113,
                orderType = "Restaurant",
                destinationAddress = "",
                pickUpByCustomer = false,
                schedulePickUpTime = "2024-04-25T11:28:44",
                originalInvoiceAmount = 55.96m,
                originalTax = 5.3162m,
                displayCompleteOrderBtn = true,
                week_Day = "Thu"
            },
            new OrderData
            {
                locationID = 5,
                restaurant = "Halal Meat And Grill",
                tP_Status = "cancelled",
                pickUpBy = "",
                pickUpByCustomerStatus = false,
                driverName = "",
                salesOrderID = 2599,
                invoiceAmount = 55.96m,
                customerName = "HD Customer",
                delivery_Status = "Pending",
                dateOrder = "2024-04-25T11:26:33.04",
                orderItems = "Behari Beef Boti - 4.00 (count)",
                customerID = 1113,
                orderType = "Restaurant",
                destinationAddress = "",
                pickUpByCustomer = false,
                schedulePickUpTime = "2024-04-25T11:28:44",
                originalInvoiceAmount = 55.96m,
                originalTax = 5.3162m,
                displayCompleteOrderBtn = true,
                week_Day = "Thu"
            }
        };

        public List<string> DeliveryStatuses { get; } = new()
        {
            "All",
            "Pending",
            "PickedUp",
            "Delivered",
            "Canceled",
            "Waiting for the customer Pickup",
            "Pickedup by customer"
        };

        public string TodaysOrdersText
        {
            get
            {
                var today = DateTime.Today;
                int todaysOrdersCount = TestOrders?.Count(o => DateTime.TryParse(o.dateOrder, out var date) && date.Date == today) ?? 0;
                return $"Today's Orders( Pending + InProgress ): {todaysOrdersCount}";
            }
        }

        [RelayCommand]
        private void NavigatedTo() =>
            _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() =>
            _isNavigatedTo = false;

        [RelayCommand]
        private async Task OpenWebsite()
        {
            var uri = new Uri("https://www.iSofware.com");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        public async void OnDeliveryStatusChanged()
        {
            try
            {
                IsLoading = true; // Start loading
                await Task.Run(() => FilterOrders()); // Run FilterOrders in a separate thread if it's a heavy operation
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDeliveryStatusChanged: {ex.Message}");
            }
            finally
            {
                IsLoading = false; // Stop loading
            }
        }

        private void FilterOrders()
        {
            if (SelectedDeliveryStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                FilteredOrders = Orders;
            }
            else
            {
                FilteredOrders = Orders?.Where(o => string.Equals(o.delivery_Status.Replace(" ", ""), SelectedDeliveryStatus.Replace(" ", ""), StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        [RelayCommand]
        public void Search()
        {
            _currentStatus = SelectedDeliveryStatus;
            if (int.TryParse(SearchText, out int salesOrderID))
            {
                FetchOrders(salesOrderID);
            }
            else
            {
                FetchOrders();
                // Show "No data found" message
                // ...update UI code...
            }
        }

        public async void FetchOrders(int? salesOrderID = null)
        {
            try
            {
                IsLoading = true;
                Console.WriteLine("Fetch Orders------------>");
                var httpClient = new HttpClient();
                var requestUri = "http://99.89.32.196/api/Buisness/BuisnesOrders";
                var requestData = new
                {
                    LocationID = 5,
                    Status = _currentStatus,
                    salesOrderID = salesOrderID
                };

                var response = await httpClient.PostAsJsonAsync(requestUri, requestData);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Response------------>");

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content------------>");
                var orders = JsonSerializer.Deserialize<OrderDataResponse>(responseContent);
                if (orders != null && orders.usersData.Any())
                {
                    Orders = orders.usersData;
                    FilterOrders(); // Update FilteredOrders based on the current status
                    var ordersJson = JsonSerializer.Serialize(Orders, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine($"User Data------------> {_currentStatus}");

                    IsDataFound = true;
                    IsNoDataFound = false;

                    switch (_currentStatus)
                    {
                        case "All": TotalNumber = Orders.Count(); ResetButtonColors(); TotalButtonColor="#AAAAAA"; Console.WriteLine("1------"); break;
                        case "New": NewNumber = Orders.Count(); ResetButtonColors(); NewButtonColor="#AAAAAA"; Console.WriteLine("2------"); break;
                        case "In Process": InProcessNumber = Orders.Count(); ResetButtonColors(); InProcessButtonColor="#AAAAAA"; Console.WriteLine("3------"); break;
                        case "Ready": ReadyNumber = Orders.Count(); ResetButtonColors(); ReadyButtonColor="#AAAAAA"; Console.WriteLine("4------"); break;
                        case "Picked Up": PickedUpNumber = Orders.Count(); ResetButtonColors(); PickedUpButtonColor="#AAAAAA"; Console.WriteLine("5------"); break;
                        case "Canceled": CanceledNumber = Orders.Count(); ResetButtonColors(); CanceledButtonColor="#AAAAAA"; Console.WriteLine("6------"); break;
                        case "Scheduled": ScheduledNumber = Orders.Count(); ResetButtonColors(); ScheduledButtonColor="#AAAAAA"; Console.WriteLine("7------"); break;
                    }

                    // Update UI with orders data
                    // ...update UI code...
                }
                else
                {
                    if (salesOrderID.HasValue)
                    {
                        IsNoDataFound = true;
                    }
                    else
                    {
                        IsPictureVisible = false;
                    }
                    IsDataFound = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders: {ex.Message}");
                if (salesOrderID.HasValue)
                {
                    IsNoDataFound = true;
                }
                else
                {
                    IsPictureVisible = false;
                }
                IsDataFound = false;
            }
            finally
            {
                IsLoading = false;

            }
        }

        public void UpdateStatus(string status)
        {
            _currentStatus = status;
            FetchOrders();
            OnPropertyChanged(nameof(TestOrders)); // Trigger UI update
        }

        [RelayCommand]

        private async Task ViewEditOrder(OrderData order)
        {
            Console.WriteLine("ViewEditOrder command executed"); // Debug log
            Console.WriteLine($"Order Type: {order.orderType}"); // Log the order type

            IsRestaurantant = false; // Reset the value initially

            try
            {
                IsLoading = true;
                Console.WriteLine("Fetch Orders------------>");
                var httpClient = new HttpClient();
                var requestUri = "http://99.89.32.196/api/Buisness/BusinessEditOrder";
                var requestData = new
                {
                    SalesOrderID = order.salesOrderID
                };

                var response = await httpClient.PostAsJsonAsync(requestUri, requestData);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Response------------>");

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content------------>");
                var orders = JsonSerializer.Deserialize<EditDataResponse>(responseContent);
                if (orders != null && orders.salesOrderItemData.Any())
                {                     
                    IsModalVisible = true;                   
                    IsEditDataFounded = true;
                    EditDatas = orders.salesOrderItemData
                        .Select(item =>
                        {
                            item.IsRestaurantant = IsRestaurantant; // Embed the value
                            return item;
                        })
                        .ToList();

                    // Update _isRestaurant based on order type
                    IsRestaurant = order.orderType?.Equals("Restaurant", StringComparison.OrdinalIgnoreCase) ?? false;
                    IsRestaurantant = !IsRestaurant; // Set the value based on IsRestaurant
                    OnPropertyChanged(nameof(IsRestaurantant));
                    Console.WriteLine($"IsRestaurant: {IsRestaurant}, IsRestaurantant: {IsRestaurantant}");

                    // Deep copy the initial data
                    InitialEditDatas = new EditDataResponse
                    {
                        salesOrderItemData = orders.salesOrderItemData
                            .Select(item => new EditData
                            {
                                rowNumber = item.rowNumber,
                                itemName = item.itemName,
                                instructions = item.instructions,
                                itemStatus = item.itemStatus,
                                salesOrderItemID = item.salesOrderItemID,
                                salesOrderID = item.salesOrderID,
                                addOnDesc = item.addOnDesc,
                                originalPrice = item.originalPrice,
                                originalAddOnPrice = item.originalAddOnPrice,
                                itemQty = item.itemQty,
                                checkoutTotal = item.checkoutTotal,
                                IsRestaurantant = IsRestaurantant // Embed the value
                            }).ToList(),
                        IsRestaurant = !IsRestaurant
                    };

                    // Assign SalesEditDatas
                    SalesEditDatas = new EditDataResponse
                    {
                        salesOrderItemData = orders.salesOrderItemData,
                        IsRestaurant = !IsRestaurant
                    };

                    var ordersJson = JsonSerializer.Serialize(InitialEditDatas, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine($"InitialEditDatas set to: {ordersJson}");
                }
                else
                {
                    IsEditDataFounded = false;
                    IsModalVisible = false;
                    var toast = Toast.Make("Cannot Load Data", ToastDuration.Short, 14);
                    await toast.Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SaveOrder()
        {
            // Save the updated order details
            // You can add logic to update the order in the list or send it to the server
            IsModalVisible = false;
        }
        
        [RelayCommand]
        private void CloseModal()
        {
            // Save the updated order details
            // You can add logic to update the order in the list or send it to the server
            IsModalVisible = false;
        }

        [RelayCommand]
        private void InitOrder()
        {
            Console.WriteLine("InitOrder command executed"); // Debug log

            if (InitialEditDatas != null && InitialEditDatas.salesOrderItemData != null)
            {
                // Reset SalesEditDatas to the initial values
                SalesEditDatas = new EditDataResponse
                {
                    salesOrderItemData = InitialEditDatas.salesOrderItemData
                        .Select(item => new EditData
                        {
                            rowNumber = item.rowNumber,
                            itemName = item.itemName,
                            instructions = item.instructions,
                            itemStatus = item.itemStatus,
                            salesOrderItemID = item.salesOrderItemID,
                            salesOrderID = item.salesOrderID,
                            addOnDesc = item.addOnDesc,
                            originalPrice = item.originalPrice,
                            originalAddOnPrice = item.originalAddOnPrice,
                            itemQty = item.itemQty,
                            checkoutTotal = item.checkoutTotal,
                            IsRestaurantant = item.IsRestaurantant
                        }).ToList(),
                    IsRestaurant = InitialEditDatas.IsRestaurant
                };

                // Notify UI about the changes
                OnPropertyChanged(nameof(SalesEditDatas));
                Console.WriteLine($"SalesEditDatas reset to initial values: {JsonSerializer.Serialize(SalesEditDatas, new JsonSerializerOptions { WriteIndented = true })}");
                Console.WriteLine($"SalesEditDatas reset to initial values: {JsonSerializer.Serialize(InitialEditDatas, new JsonSerializerOptions { WriteIndented = true })}");
            }
        }

#if ANDROID
        public void TestRepeatingNotification()
        {
            var notificationService = new NotificationManagerService();
            notificationService.ScheduleRepeatingNotification("Repeating Title", "This notification repeats every 2 minutes.", 1);
            Console.WriteLine($"ScheduleRepeatingNotification: Scheduled notification with title and message to repeat every minutes.");
        }

        public void TestPopupNotification()
        {
            try
            {
                Console.WriteLine("Testing popup notification...");
                var notificationService = new NotificationManagerService();
                notificationService.ShowPopupNotification("Incoming Call", "John Doe is calling you");
                Console.WriteLine("Popup notification method called successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestPopupNotification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
#endif

        private string _totalButtonColor = "#4CAF50";
        private string _newButtonColor = "#FFC107";
        private string _inProcessButtonColor = "#8BC34A";
        private string _readyButtonColor = "#03A9F4";
        private string _pickedUpButtonColor = "#F44336";
        private string _canceledButtonColor = "#9C27B0";
        private string _scheduledButtonColor = "#673AB7";

        public string TotalButtonColor
        {
            get => _totalButtonColor;
            set
            {
                _totalButtonColor = value;
                OnPropertyChanged();
            }
        }

        public string NewButtonColor
        {
            get => _newButtonColor;
            set
            {
                _newButtonColor = value;
                OnPropertyChanged();
            }
        }

        public string InProcessButtonColor
        {
            get => _inProcessButtonColor;
            set
            {
                _inProcessButtonColor = value;
                OnPropertyChanged();
            }
        }

        public string ReadyButtonColor
        {
            get => _readyButtonColor;
            set
            {
                _readyButtonColor = value;
                OnPropertyChanged();
            }
        }

        public string PickedUpButtonColor
        {
            get => _pickedUpButtonColor;
            set
            {
                _pickedUpButtonColor = value;
                OnPropertyChanged();
            }
        }

        public string CanceledButtonColor
        {
            get => _canceledButtonColor;
            set
            {
                _canceledButtonColor = value;
                OnPropertyChanged();
            }
        }

        public string ScheduledButtonColor
        {
            get => _scheduledButtonColor;
            set
            {
                _scheduledButtonColor = value;
                OnPropertyChanged();
            }
        }

        private void OnStatusButtonClicked(object sender, EventArgs e)
        {
            ResetButtonColors();
            if (sender is Button button)
            {
                button.BackgroundColor = Colors.Gray;
            }
        }

        private void ResetButtonColors()
        {
            TotalButtonColor = "#4CAF50";
            NewButtonColor = "#FFC107";
            InProcessButtonColor = "#8BC34A";
            ReadyButtonColor = "#03A9F4";
            PickedUpButtonColor = "#F44336";
            CanceledButtonColor = "#9C27B0";
            ScheduledButtonColor = "#673AB7";
        }

        [RelayCommand]
        public async Task FetchAndShowNotifications()
        {
            // await _notificationService.FetchAndShowNotificationsAsync();
        }
        public bool CanEditCheckoutTotal => IsRestaurant && SelectedDeliveryStatus == "Pending";

        partial void OnIsRestaurantChanged(bool value)
        {
            Console.WriteLine($"IsRestaurant changed to: {value}");
    Console.WriteLine($"CanEditCheckoutTotal: {CanEditCheckoutTotal}");
            OnPropertyChanged(nameof(CanEditCheckoutTotal));
        }

        partial void OnSelectedDeliveryStatusChanged(string value)
        {    Console.WriteLine($"SelectedDeliveryStatus changed to: {value}");
    Console.WriteLine($"CanEditCheckoutTotal: {CanEditCheckoutTotal}");
            OnPropertyChanged(nameof(CanEditCheckoutTotal));
        }

    }
}
