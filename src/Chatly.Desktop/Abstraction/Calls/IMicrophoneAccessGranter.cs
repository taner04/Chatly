using Avalonia.Platform;

namespace Chatly.Desktop.Abstraction.Calls;

public interface IMicrophoneAccessGranter
{
    void GrantAccess(IPlatformHandle webViewHandle, Uri pageAddress);
}