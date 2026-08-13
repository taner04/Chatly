using Chatly.Desktop.Shared.ViewModels.Base;

namespace Chatly.Desktop.Features.Home.ViewModels;

public sealed class HomePageViewModel : ViewModelBase
{
    public string Title => "Home";

    public string Description => "This is the landing page for your Avalonia shell.";
}