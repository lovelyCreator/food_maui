using System.Text.Json.Serialization;
using CommunityToolkit.Maui.Core.Extensions;
using System;
using System.Collections.Generic;

namespace Food_maui.Models
{
    public class UserData
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? LoginBy { get; set; }
        public int? LocationID { get; set; }
        public string? UserID { get; set; }
        public string? RestaurantType { get; set; }
        public string? Restaurant { get; set; }
        public string? BusinessOwner { get; set; }
        public string? RoleName { get; set; }
        public string? TaxPercentage { get; set; }
        public string? Zip { get; set; }
        public string? DeliveryManagedBy { get; set; }
        public string? HdBusinessRulesChargeType { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }

    public class LoginResponse
    {
        public List<UserData>? UsersData { get; set; }
        public List<ErrorData>? ErrorData { get; set; } // Add ErrorData property
    }

    public class ErrorData
    {
        public string? ErrorSubject { get; set; }
        public string? ErrorValue { get; set; }
    }
}