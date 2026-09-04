using FluentIcons.Common;

namespace Chatly.Desktop.Abstraction.Toasts;

public interface IToastViewModel
{
    Guid Id { get; }

    string Title { get; set; }
    string Message { get; set; }
    Symbol Icon { get; set; }
    event EventHandler Dismissed;
}
