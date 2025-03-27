using System.Text.Json.Serialization;
using CommunityToolkit.Maui.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Food_maui.Models
{
    public class EditData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double? _originalPrice;
        private double? _originalAddOnPrice;
        private double? _itemQty;

        public string? rowNumber { get; set; }
        public string? itemName { get; set; }
        public string? instructions { get; set; }
        public string? itemStatus { get; set; }
        public string? salesOrderItemID { get; set; }
        public string? salesOrderID { get; set; }
        public string? addOnDesc { get; set; }
        public bool? IsRestaurantant { get; set; }

        public double? originalPrice
        {
            get => _originalPrice;
            set
            {
                _originalPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemTotal));
            }
        }

        public double? originalAddOnPrice
        {
            get => _originalAddOnPrice;
            set
            {
                _originalAddOnPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AddOnTotal));
            }
        }

        public double? itemQty
        {
            get => _itemQty;
            set
            {
                _itemQty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemTotal));
                OnPropertyChanged(nameof(AddOnTotal));
            }
        }

        public double? checkoutTotal { get; set; }

        // Calculated properties
        public double AddOnTotal => (itemQty == null ? 0 : (originalAddOnPrice ?? 0) * (itemQty ?? 0));
        public double ItemTotal => (itemQty == null ? 0 : (originalPrice ?? 0) * (itemQty ?? 0));

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EditDataResponse
    {
        public List<EditData> salesOrderItemData { get; set; }
        public bool? IsRestaurant { get; set; }
    }
}
