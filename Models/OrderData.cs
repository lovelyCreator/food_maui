using System.Text.Json.Serialization;
using CommunityToolkit.Maui.Core.Extensions;
using System;
using System.Collections.Generic;

namespace Food_maui.Models
{
    public class OrderData
    {
        public int? locationID { get; set; }
        public string? restaurant { get; set; }
        public string? tP_Status { get; set; }
        public string? pickUpBy { get; set; }
        public bool? pickUpByCustomerStatus { get; set; }
        public string? driverName { get; set; }
        public int? salesOrderID { get; set; }
        public decimal? invoiceAmount { get; set; }
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
        public decimal? originalInvoiceAmount { get; set; }
        public decimal? originalTax { get; set; }
        public bool? displayCompleteOrderBtn { get; set; }
        public string? week_Day { get; set; }

        public string ActionButtonText => delivery_Status?.Equals("Canceled", StringComparison.OrdinalIgnoreCase) == true
            ? "View/Print"
            : "View/Edit";
        
    }

    public class OrderDataResponse
    {
        public List<OrderData> usersData { get; set; }
    }
}