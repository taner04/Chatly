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
        ToastType type = ToastType.Information)
    {
        Id = Guid.CreateVersion7();

        Title = title;
        Message = message;
        Icon = icon;
        Type = type;
    }

    public ToastType Type { get; }
    public bool IsSuccess => Type == ToastType.Success;
    public bool IsError => Type == ToastType.Error;

    public Guid Id { get; init; }

    public string Title { get; set; }
    public string Message { get; set; }
    public Symbol Icon { get; set; }

    public event EventHandler? Dismissed;

    [RelayCommand]
    private void Dismiss()
    {
        Dismissed?.Invoke(this, EventArgs.Empty);
    }
}
