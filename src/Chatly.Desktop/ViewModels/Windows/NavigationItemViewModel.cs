using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Windows;

public sealed partial class NavigationItemViewModel(
    string title,
    Symbol icon,
    Type viewModelType,
    Action navigate) : ViewModelBase
{
    public string Title { get; } = title;

    public Symbol Icon { get; } = icon;

    public Type ViewModelType { get; } = viewModelType;

    public IRelayCommand NavigateCommand { get; } = new RelayCommand(navigate);

    [ObservableProperty] public partial bool IsSelected { get; set; }

    public static NavigationItemViewModel Create<T>(
        string title,
        Symbol icon,
        INavigationService navigationService) where T : INavigableViewModel
    {
        return new NavigationItemViewModel(title, icon, typeof(T), () => navigationService.NavigateTo<T>());
    }
}