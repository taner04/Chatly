using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Windows;

public sealed partial class NavigationItemViewModel(
    string title,
    Symbol icon,
    Type viewModelType,
    Func<Task> navigate) : ViewModelBase
{
    public string Title { get; } = title;

    public Symbol Icon { get; } = icon;

    public Type ViewModelType { get; } = viewModelType;

    public IAsyncRelayCommand NavigateCommand { get; } = new AsyncRelayCommand(navigate);

    [ObservableProperty] public partial bool IsSelected { get; set; }

    internal static NavigationItemViewModel Create<T>(
        string title,
        Symbol icon,
        INavigationService navigationService) where T : INavigableViewModel
    {
        return new NavigationItemViewModel(title, icon, typeof(T), () => navigationService.NavigateToAsync<T>());
    }
}
