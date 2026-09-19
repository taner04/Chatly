using FluentIcons.Common;

namespace Chatly.Desktop.Abstraction.Toasts;

public interface IToastViewModel
{
    Guid Id { get; }

    string Title { get; }
    string Message { get; }
    Symbol Icon { get; }
    event EventHandler? Dismissed;
}