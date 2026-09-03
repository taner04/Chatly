using Chatly.Desktop.Services.Toasts;
using FluentIcons.Common;

namespace Chatly.Desktop.Abstraction.Toasts;

public interface IToastViewModel
{
    Guid Id { get; }

    string Title { get; set; }
    string Message { get; set; }
    Symbol Icon { get; set; }
    IReadOnlyCollection<ToastButton> Buttons { get; set; }

    event EventHandler<ToastButtonClickedEventArgs> ButtonClicked;
}