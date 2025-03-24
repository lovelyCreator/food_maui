using CommunityToolkit.Maui.Views;
using Food_maui.Models;
using Food_maui.PageModels;
using Food_maui.Services;
using LocalNotificationDemo.Platforms.Android;
using Microsoft.Maui.Controls;
using System;

namespace Food_maui.Pages;

public partial class MainPage : ContentPage
{
    private readonly UserMetadataService _userMetadataService;
    public string? UserName { get; set; }

    public MainPage(MainPageModel model, UserMetadataService userMetadataService)
    {
        InitializeComponent();
        BindingContext = model;
        _userMetadataService = userMetadataService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Access user metadata
        if (!string.IsNullOrEmpty(_userMetadataService.UserID))
        {
            Console.WriteLine($"UserName from service: {_userMetadataService.UserID}");
        }
        else
        {
            Console.WriteLine("UserName not found in service.");
        }
    }

    private void OnTestNotificationClicked(object sender, EventArgs e)
    {
        TestRepeatingNotification();
    }


    private void TestNotification()
    {
        var notificationService = new NotificationManagerService();
        notificationService.ShowNotification("Test Title", "This is a test notification.");
        Console.WriteLine($"ShowNotification: Notification displayed with title:  and message: ");
    }

    private void TestRepeatingNotification()
    {
        var notificationService = new NotificationManagerService();
        notificationService.ScheduleRepeatingNotification("Repeating Title", "This notification repeats every 2 minutes.", 2);
        Console.WriteLine($"ScheduleRepeatingNotification: Scheduled notification with title and message to repeat every minutes.");
    }
}