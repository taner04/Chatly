using System;
using System.Windows.Input;

namespace Chatly.Desktop.ViewModels;

public sealed class NavigationItemViewModel(string title, Type page) : ViewModelBase
{
    private bool _isSelected;

    public string Title { get; } = title;

    public Type Page { get; } = page;


    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}