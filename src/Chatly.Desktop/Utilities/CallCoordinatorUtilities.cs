using System.Runtime.CompilerServices;

namespace Chatly.Desktop.Utilities;

internal static class CallCoordinatorUtilities
{
    internal static async Task TryInvokeAsync(
        Func<Task> operation,
        Action<Exception, string> failureHandler,
        [CallerMemberName] string caller = "")
    {
        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            failureHandler(exception, caller);
        }
    }
}