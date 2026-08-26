using System;
using Chatly.Desktop.Abstractions.Navigation;

namespace Chatly.Desktop.Services.Navigation;

public abstract class NavigationEntry(Type viewModelType)
{
    public Type ViewModelType { get; } = viewModelType;

    public abstract void ShowPage(INavigationView navigationView);
}

public sealed class NavigationEntry<T>(INavigableView<T> view)
    : NavigationEntry(typeof(T)) where T : INavigableViewModel
{
    public override void ShowPage(INavigationView navigationView)
    {
        navigationView.SetPage(view);
    }
}
