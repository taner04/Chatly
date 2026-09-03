using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Services.Toasts;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

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

    public ToastType Type { get; }
    public bool IsSuccess => Type == ToastType.Success;
    public bool IsError => Type == ToastType.Error;

    public Guid Id { get; init; }

    public string Title { get; set; }
    public string Message { get; set; }
    public Symbol Icon { get; set; }
    public IReadOnlyCollection<ToastButton> Buttons { get; set; }

    public event EventHandler<ToastButtonClickedEventArgs>? ButtonClicked;

    [RelayCommand]
    private void Dismiss()
    {
        ButtonClicked?.Invoke(this, new ToastButtonClickedEventArgs(null, true));
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        if (sender is ToastButton button)
        {
            ButtonClicked?.Invoke(this, new ToastButtonClickedEventArgs(button));
        }
    }
}