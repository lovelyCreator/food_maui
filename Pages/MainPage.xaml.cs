using CommunityToolkit.Maui.Views;
using Food_maui.Models;
using Food_maui.PageModels;
using Food_maui.Services;
#if ANDROID
using LocalNotificationDemo.Platforms.Android;
#endif
using Microsoft.Maui.Controls;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.ComponentModel;
using System.Linq;

namespace Food_maui.Pages;

public partial class MainPage : ContentPage
{
    private readonly UserMetadataService _userMetadataService;
    public string? UserName { get; set; }

    public MainPage(MainPageModel model, UserMetadataService userMetadataService)
    {
        try
        {
            InitializeComponent();
            if (model == null) throw new ArgumentNullException(nameof(model), "MainPageModel cannot be null.");
            if (userMetadataService == null) throw new ArgumentNullException(nameof(userMetadataService), "UserMetadataService cannot be null.");

            BindingContext = model;
            _userMetadataService = userMetadataService;

            // Subscribe to property changed event to log Orders property
            model.PropertyChanged += Model_PropertyChanged;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MainPage constructor: {ex}");
            throw;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Access user metadata
            if (!string.IsNullOrEmpty(_userMetadataService?.UserID))
            {
                Console.WriteLine($"UserName from service: {_userMetadataService.UserID}");
            }
            else
            {
                Console.WriteLine("UserName not found in service.");
            }

            // Fetch orders with status "All" and no salesOrderID
            ((MainPageModel)BindingContext)?.FetchOrders();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnAppearing: {ex.Message}");
            throw;
        }
    }

    private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainPageModel.Orders))
        {
            var orders = ((MainPageModel)BindingContext).Orders;
            var ordersJson = JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true });
            // Console.WriteLine($"Orders updated: {ordersJson}");
        }
    }

    private void OnTestNotificationClicked(object sender, EventArgs e)
    {
#if ANDROID
        ((MainPageModel)BindingContext).TestRepeatingNotification();
#endif
    }

    private void OnTestPopupNotificationClicked(object sender, EventArgs e)
    {
#if ANDROID
        ((MainPageModel)BindingContext).TestPopupNotification();
#endif
    }

    private void OnStatusButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var status = button.Text.Split(' ')[0];
            var viewModel = BindingContext as MainPageModel;
            if (viewModel != null)
            {
                viewModel.SelectedDeliveryStatus = status switch
                {
                    "All" => "All",
                    "Pending" => "Pending",
                    "PickedUp" => "PickedUp",
                    "Delivered" => "Delivered",
                    "Canceled" => "Canceled",
                    "In" => "In Process",
                    "Picked" => "Picked Up",
                    _ => "All"
                };
                viewModel.SearchCommand.Execute(null);
            }
        }
    }

    private void OnViewEditButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var orderData = button?.BindingContext as OrderData;
        if (orderData != null)
        {
            var viewModel = BindingContext as MainPageModel;
            viewModel?.ViewEditOrderCommand.Execute(orderData);
        }
    }

    private void UpdateButtonBorders(string selectedStatus)
    {
        foreach (var child in StatusButtonsContainer.Children)
        {
            if (child is Button button)
            {
                button.BorderWidth = button.Text.Split(' ')[0] == selectedStatus ? 2 : 0;
            }
        }
    }

    private void OnSearchButtonClicked(object sender, EventArgs e)
    {
        ((MainPageModel)BindingContext).Search();
    }

    private void OnSearchEntryCompleted(object sender, EventArgs e)
    {
        if (BindingContext is MainPageModel viewModel)
        {
            viewModel.Search();
        }
    }

    private void OnDeliveryStatusChanged(object sender, EventArgs e)
    {
        if (BindingContext is MainPageModel viewModel)
        {
            viewModel.OnDeliveryStatusChanged();
        }
    }

    private void OnInitButtonClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Init button clicked.");
        var viewModel = BindingContext as MainPageModel;
        if (viewModel != null)
        {
            viewModel.InitOrderCommand.Execute(null);
        }
    }

    private void OnItemQtyChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is EditData editData)
        {
            if (double.TryParse(entry.Text, out double newQty))
            {
                editData.itemQty = newQty;

                // Notify changes to the SalesEditDatas collection
                var mainPageModel = BindingContext as MainPageModel;
                if (mainPageModel != null)
                {
                    var updatedItem = mainPageModel.SalesEditDatas.salesOrderItemData
                        .FirstOrDefault(item => item.rowNumber == editData.rowNumber);
                    if (updatedItem != null)
                    {
                        updatedItem.itemQty = newQty;
                        updatedItem.checkoutTotal = updatedItem.originalPrice * newQty;

                        // Log the updated SalesEditDatas to the console
                        var salesEditDatasJson = JsonSerializer.Serialize(mainPageModel.SalesEditDatas, new JsonSerializerOptions { WriteIndented = true });
                        Console.WriteLine($"Updated SalesEditDatas: {salesEditDatasJson}");
                    }
                }
            }
        }
    }
}