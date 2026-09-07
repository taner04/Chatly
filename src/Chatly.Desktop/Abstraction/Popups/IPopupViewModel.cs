namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupViewModel
{
    string Title { get; }
    Task Completion { get; }

    public void CloseOverlay();
}