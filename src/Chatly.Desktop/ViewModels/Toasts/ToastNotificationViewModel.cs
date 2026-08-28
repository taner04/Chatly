using Chatly.Desktop.Abstractions.Toasts;
using Chatly.Desktop.Services.Toasts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using System;
using System.Collections.Generic;

namespace Chatly.Desktop.ViewModels.Toasts;

public sealed partial class ToastNotificationViewModel : ViewModelBase, IToastViewModel
{
    public ToastNotificationViewModel(
        string title,
        string message,
        Symbol icon,
        ToastType type = ToastType.Information,
        IReadOnlyCollection<ToastButton>? buttons = null)
    {
        Id = Guid.CreateVersion7();

        Title = title;
        Message = message;
        Icon = icon;
        Type = type;
        Buttons = buttons ?? [];

        foreach (var button in Buttons)
        {
            button.Clicked += OnButtonClicked;
        }
    }

    public Guid Id { get; init; }

    public string Title { get; set; }
    public string Message { get; set; }
    public Symbol Icon { get; set; }
    public ToastType Type { get; }
    public bool IsSuccess => Type == ToastType.Success;
    public bool IsError => Type == ToastType.Error;
    public IReadOnlyCollection<ToastButton> Buttons { get; set; }

    public event EventHandler<ToastButtonClickedEventArgs>? ButtonClicked;

    [RelayCommand]
    private void Dismiss()
    {
        ButtonClicked?.Invoke(this, new ToastButtonClickedEventArgs(null, isDismissed: true));
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        if (sender is ToastButton button)
        {
            ButtonClicked?.Invoke(this, new ToastButtonClickedEventArgs(button));
        }
    }
}
