using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Windows;

public sealed partial class NavigationItemViewModel(
    string title,
    Symbol icon,
    Type viewModelType,
    INavigationService navigationService) : ViewModelBase
{
    public string Title { get; } = title;

    public Symbol Icon { get; } = icon;

    public Type ViewModelType { get; } = viewModelType;

    [ObservableProperty] public partial bool IsSelected { get; set; }

    [RelayCommand]
    private async Task NavigateAsync(CancellationToken cancellationToken)
    {
        await navigationService.NavigateToAsync(ViewModelType, cancellationToken);
    }

    internal static NavigationItemViewModel Create<T>(string title, Symbol icon, INavigationService navigationService)
        where T : INavigableViewModel =>
        new(title, icon, typeof(T), navigationService);
}