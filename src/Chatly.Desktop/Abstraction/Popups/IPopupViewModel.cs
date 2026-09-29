namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupViewModel
{
    string Title { get; }

    bool IsDismissible { get; }

    Task Completion { get; }

    void CloseOverlay();
}