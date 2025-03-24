using CommunityToolkit.Maui.Views;
using Food_maui.Models;
using Food_maui.PageModels;
using Food_maui.Services;
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
        if (!string.IsNullOrEmpty(_userMetadataService.UserName))
        {
            Console.WriteLine($"UserName from service: {_userMetadataService.UserName}");
        }
        else
        {
            Console.WriteLine("UserName not found in service.");
        }
    }
}