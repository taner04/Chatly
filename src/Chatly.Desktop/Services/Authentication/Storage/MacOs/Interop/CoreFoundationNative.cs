using System.Runtime.InteropServices;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

internal static unsafe partial class CoreFoundationNative
{
    private const string CoreFoundationFramework = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [LibraryImport(CoreFoundationFramework)]
    internal static partial nint CFDictionaryCreateMutable(
        nint allocator,
        nint capacity,
        nint keyCallBacks,
        nint valueCallBacks);

    [LibraryImport(CoreFoundationFramework)]
    internal static partial void CFDictionarySetValue(
        nint dictionary,
        nint key,
        nint value);

    [LibraryImport(CoreFoundationFramework)]
    internal static partial nint CFStringCreateWithCString(
        nint allocator,
        byte* cStr,
        uint encoding);

    [LibraryImport(CoreFoundationFramework)]
    internal static partial nint CFDataCreate(
        nint allocator,
        byte* bytes,
        nint length);

    [LibraryImport(CoreFoundationFramework)]
    internal static partial nint CFDataGetBytePtr(
        nint data);

    [LibraryImport(CoreFoundationFramework)]
    internal static partial nint CFDataGetLength(
        nint data);

    [LibraryImport(CoreFoundationFramework)]
    internal static partial void CFRelease(nint cf);
}