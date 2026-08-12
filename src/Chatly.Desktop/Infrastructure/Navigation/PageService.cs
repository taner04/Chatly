using System;
using Avalonia.Controls;
using Chatly.Desktop.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Infrastructure.Navigation;

public sealed class PageService(IServiceProvider serviceProvider)
{
    public INavigavablePage GetPage(Type pageType)
    {
        if (!typeof(Control).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"{pageType.FullName} must derive from {nameof(Control)}.", nameof(pageType));
        }

        return (INavigavablePage)serviceProvider.GetRequiredService(pageType);
    }
}