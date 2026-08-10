using System;
using Avalonia.Controls;
using Chatly.Desktop.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Infrastructure;

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
