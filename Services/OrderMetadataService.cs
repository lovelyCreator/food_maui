namespace Food_maui.Services
{
    public class OrderMetadataService
    {
        public int? locationID { get; set; }
        public string? restaurant { get; set; }
        public string? tP_Status { get; set; }
        public string? pickUpBy { get; set; }
        public string? pickUpByCustomerStatus { get; set; }
        public string? driverName { get; set; }
        public int? salesOrderID { get; set; }
        public int? invoiceAmount { get; set; }
        public string? customerName { get; set; }
        public string? Pending { get; set; }
        public string? delivery_Status { get; set; }
        public string? dateOrder { get; set; }
        public string? orderItems { get; set; }
        public int? customerID { get; set; }
        public string? orderType { get; set; }
        public string? destinationAddress { get; set; }
        public bool? pickUpByCustomer { get; set; }
        public string? schedulePickUpTime { get; set; }
        public int? originalInvoiceAmount { get; set; }
        public int? originalTax { get; set; }
        public bool? displayCompleteOrderBtn { get; set; }
        public string? week_Day { get; set; }

        public OrderMetadataService()
        {
            // Initialize UserName and UserID with empty strings
            locationID = 1;
            restaurant = "restaurant";
            
        }

        // Method to save user data persistently
        public async Task SaveUserDataAsync()
        {
            // Implement persistent storage logic here (e.g., save to file, database, or secure storage)
            await Task.CompletedTask;
        }

        // Method to load user data persistently
        public async Task LoadUserDataAsync()
        {
            // Implement logic to load user data from persistent storage
            await Task.CompletedTask;
        }
    }
}