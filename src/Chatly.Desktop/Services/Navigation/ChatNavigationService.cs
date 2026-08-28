using System.Collections.Generic;

namespace Chatly.Desktop.Services.Navigation;

internal sealed class ChatNavigationService
{
    private readonly Stack<NavigationEntry> _backStack = new();
    private readonly Stack<NavigationEntry> _forwardStack = new();
}
