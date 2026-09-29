using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Platform;
using Chatly.Desktop.Abstraction.Calls;

namespace Chatly.Desktop.Services.Calls.Media.Permissions.MacOs;

[SupportedOSPlatform("macos")]
internal sealed unsafe partial class MacOsMicrophoneAccessGranter : IMicrophoneAccessGranter
{
    private const string ObjectiveC = "/usr/lib/libobjc.A.dylib";
    private const string DelegateClassName = "ChatlyMediaPermissionDelegate";
    private const string MethodTypes = "v@:@@@q@?";
    private const nint MicrophoneCaptureType = 1;
    private const nint PermissionPrompt = 0;
    private const nint PermissionGrant = 1;

    private static readonly Lock Sync = new();
    private static IntPtr _ownDelegate;
    private static int _allowedPort;
    private static string? _allowedHost;

    private static IntPtr Implementation =>
        (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, nint, IntPtr, void>)
        &RequestMediaCapturePermission;

    public void GrantAccess(IPlatformHandle webViewHandle, Uri pageAddress)
    {
        if (webViewHandle is not IAppleWKWebViewPlatformHandle appleHandle)
        {
            throw new PlatformNotSupportedException("The web view is not a WKWebView.");
        }

        var webView = appleHandle.WKWebView;
        lock (Sync)
        {
            Volatile.Write(ref _allowedHost, pageAddress.Host);
            Volatile.Write(ref _allowedPort, pageAddress.Port);

            DisableFocusRequirement(webView);

            var selector =
                Selector("webView:requestMediaCapturePermissionForOrigin:initiatedByFrame:type:decisionHandler:");
            var uiDelegate = SendReturningPointer(webView, Selector("UIDelegate"));
            if (uiDelegate == IntPtr.Zero)
            {
                uiDelegate = GetOrCreateOwnDelegate(selector);
                SendWithPointer(webView, Selector("setUIDelegate:"), uiDelegate);
                return;
            }

            var delegateClass = object_getClass(uiDelegate);
            if (!class_respondsToSelector(delegateClass, selector))
            {
                class_addMethod(delegateClass, selector, Implementation, MethodTypes);
            }
        }
    }

    private static void DisableFocusRequirement(IntPtr webView)
    {
        var preferences = SendReturningPointer(
            SendReturningPointer(webView, Selector("configuration")),
            Selector("preferences"));
        var setter = Selector("_setGetUserMediaRequiresFocus:");
        if (class_respondsToSelector(object_getClass(preferences), setter))
        {
            SendWithBool(preferences, setter, false);
        }
    }

    private static IntPtr GetOrCreateOwnDelegate(IntPtr selector)
    {
        if (_ownDelegate != IntPtr.Zero)
        {
            return _ownDelegate;
        }

        var delegateClass = objc_getClass(DelegateClassName);
        if (delegateClass == IntPtr.Zero)
        {
            delegateClass = objc_allocateClassPair(objc_getClass("NSObject"), DelegateClassName, 0);
            class_addMethod(delegateClass, selector, Implementation, MethodTypes);
            objc_registerClassPair(delegateClass);
        }

        _ownDelegate = SendReturningPointer(SendReturningPointer(delegateClass, Selector("alloc")), Selector("init"));
        return _ownDelegate;
    }

    [UnmanagedCallersOnly]
    private static void RequestMediaCapturePermission(
        IntPtr self,
        IntPtr command,
        IntPtr webView,
        IntPtr origin,
        IntPtr frame,
        nint type,
        IntPtr decisionHandler)
    {
        var decision = PermissionPrompt;
        try
        {
            var host = Marshal.PtrToStringUTF8(
                SendReturningPointer(SendReturningPointer(origin, Selector("host")), Selector("UTF8String")));
            var port = (int)SendReturningPointer(origin, Selector("port"));
            if (type == MicrophoneCaptureType && host == Volatile.Read(ref _allowedHost) &&
                port == Volatile.Read(ref _allowedPort))
            {
                decision = PermissionGrant;
            }
        }
        catch
        {
            decision = PermissionPrompt;
        }

        var invoke = *(IntPtr*)(decisionHandler + 16);
        ((delegate* unmanaged<IntPtr, nint, void>)invoke)(decisionHandler, decision);
    }

    private static IntPtr Selector(string name) => sel_registerName(name);

    [LibraryImport(ObjectiveC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr sel_registerName(string name);

    [LibraryImport(ObjectiveC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_getClass(string name);

    [LibraryImport(ObjectiveC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_allocateClassPair(IntPtr superclass, string name, nint extraBytes);

    [LibraryImport(ObjectiveC)]
    private static partial void objc_registerClassPair(IntPtr cls);

    [LibraryImport(ObjectiveC)]
    private static partial IntPtr object_getClass(IntPtr obj);

    [LibraryImport(ObjectiveC)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool class_respondsToSelector(IntPtr cls, IntPtr selector);

    [LibraryImport(ObjectiveC, StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool class_addMethod(IntPtr cls, IntPtr selector, IntPtr implementation, string types);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendReturningPointer(IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial void SendWithPointer(IntPtr receiver, IntPtr selector, IntPtr argument);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial void SendWithBool(
        IntPtr receiver,
        IntPtr selector,
        [MarshalAs(UnmanagedType.I1)] bool argument);
}