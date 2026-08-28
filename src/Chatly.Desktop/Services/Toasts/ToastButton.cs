using CommunityToolkit.Mvvm.Input;
using System;

namespace Chatly.Desktop.Services.Toasts;

public sealed partial class ToastButton(ToastButtonType type, IRelayCommand command)
{
    public ToastButtonType Type { get; } = type;
    public string Text { get; set; } = type.ToString();
    public IRelayCommand Command { get; set; } = command;

    public event EventHandler? Clicked;

    [RelayCommand]
    private void Click()
    {
        if (Command.CanExecute(null))
        {
            Command.Execute(null);
        }

        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
