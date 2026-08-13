using System;
using Chatly.Desktop.Shared.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace Chatly.Desktop.Features.Shell.ViewModels;

public sealed partial class NavigationItemViewModel(string title, Symbol icon, Type page) : ViewModelBase
{
    public string Title { get; } = title;

    public Symbol Icon { get; } = icon;

    public Type Page { get; } = page;

    [ObservableProperty] public partial bool IsSelected { get; set; }
}