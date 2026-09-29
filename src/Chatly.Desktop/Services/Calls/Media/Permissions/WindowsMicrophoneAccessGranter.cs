using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Platform;
using Chatly.Desktop.Abstraction.Calls;

namespace Chatly.Desktop.Services.Calls.Media.Permissions;

[SupportedOSPlatform("windows")]
internal sealed unsafe class WindowsMicrophoneAccessGranter : IMicrophoneAccessGranter
{
    private const int AddPermissionRequestedSlot = 23;
    private const int GetUriSlot = 3;
    private const int GetPermissionKindSlot = 4;
    private const int PutStateSlot = 7;
    private const int MicrophonePermissionKind = 1;
    private const int AllowPermissionState = 1;
    private const int Ok = 0;

    private static readonly Lock Sync = new();
    private static readonly HashSet<IntPtr> RegisteredWebViews = [];
    private static IntPtr _handler;
    private static int _allowedPort;
    private static string? _allowedHost;

    public void GrantAccess(IPlatformHandle webViewHandle, Uri pageAddress)
    {
        if (webViewHandle is not IWindowsWebView2PlatformHandle webView2Handle)
        {
            throw new PlatformNotSupportedException("The web view is not a WebView2 control.");
        }

        var coreWebView = webView2Handle.CoreWebView2;
        lock (Sync)
        {
            Volatile.Write(ref _allowedHost, pageAddress.Host);
            Volatile.Write(ref _allowedPort, pageAddress.Port);

            if (!RegisteredWebViews.Add(coreWebView))
            {
                return;
            }

            long token;
            var addPermissionRequested =
                (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, long*, int>)VirtualMethod(coreWebView,
                    AddPermissionRequestedSlot);
            Marshal.ThrowExceptionForHR(addPermissionRequested(coreWebView, GetOrCreateHandler(), &token));
        }
    }

    private static IntPtr GetOrCreateHandler()
    {
        if (_handler != IntPtr.Zero)
        {
            return _handler;
        }

        var vtable = (IntPtr*)NativeMemory.Alloc((nuint)(4 * sizeof(IntPtr)));
        vtable[0] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint>)&Release;
        vtable[3] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int>)&Invoke;

        var instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;
        _handler = (IntPtr)instance;
        return _handler;
    }

    private static IntPtr VirtualMethod(IntPtr comObject, int slot) => (*(IntPtr**)comObject)[slot];

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(IntPtr self, Guid* interfaceId, IntPtr* result)
    {
        *result = self;
        return Ok;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(IntPtr self) => 1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IntPtr self) => 1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Invoke(IntPtr self, IntPtr sender, IntPtr arguments)
    {
        try
        {
            int permissionKind;
            var getPermissionKind =
                (delegate* unmanaged[Stdcall]<IntPtr, int*, int>)VirtualMethod(arguments, GetPermissionKindSlot);
            if (getPermissionKind(arguments, &permissionKind) != Ok || permissionKind != MicrophonePermissionKind)
            {
                return Ok;
            }

            IntPtr uriPointer;
            var getUri = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)VirtualMethod(arguments, GetUriSlot);
            if (getUri(arguments, &uriPointer) != Ok)
            {
                return Ok;
            }

            var uri = Marshal.PtrToStringUni(uriPointer);
            Marshal.FreeCoTaskMem(uriPointer);

            if (Uri.TryCreate(uri, UriKind.Absolute, out var origin)
                && origin.Host == Volatile.Read(ref _allowedHost)
                && origin.Port == Volatile.Read(ref _allowedPort))
            {
                var putState = (delegate* unmanaged[Stdcall]<IntPtr, int, int>)VirtualMethod(arguments, PutStateSlot);
                putState(arguments, AllowPermissionState);
            }
        }
        catch
        {
            // ignored
        }

        return Ok;
    }
}