using Avalonia.Threading;

namespace Chatly.Desktop.Utilities;

internal static class UiThreadDispatcher
{
    private static Dispatcher Dispatcher => Dispatcher.UIThread;

    public static void SafeInvoke(Action action)
    {
        if (Dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.Invoke(action);
        }
    }

    public static async Task SafeInvokeAsync(Func<Task> action)
    {
        if (Dispatcher.CheckAccess())
        {
            await action();
        }
        else
        {
            await Dispatcher.InvokeAsync(action);
        }
    }
}
