using System;
using Avalonia.Controls;
using Chatly.Desktop.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Infrastructure.Navigation;

public sealed class PageService(IServiceProvider serviceProvider)
{
    public INavigablePage GetPage(Type pageType)
    {
        if (!typeof(Control).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"{pageType.FullName} must derive from {nameof(Control)}.", nameof(pageType));
        }

        if (!typeof(INavigablePage).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"{pageType.FullName} must implement {nameof(INavigablePage)}.", nameof(pageType));
        }

        return (INavigablePage)serviceProvider.GetRequiredService(pageType);
    }
}
