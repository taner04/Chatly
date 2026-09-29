using Chatly.Desktop.Services.Toasts;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Toasts;

public sealed partial class ToastViewModel : ViewModelBase
{
    internal ToastViewModel(
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

    public Guid Id { get; }

    public string Title { get; }
    public string Message { get; }
    public Symbol Icon { get; }

    public event EventHandler? Dismissed;

    [RelayCommand]
    private void Dismiss()
    {
        Dismissed?.Invoke(this, EventArgs.Empty);
    }
}