namespace Chatly.Desktop.Infrastructure.Authentication.Storage.MacOs.Native;

using System.Runtime.InteropServices;

internal static partial class CoreFoundationNative
{
    internal const string Library =
        "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    internal const uint StringEncodingUtf8 = 0x08000100;

    /// <summary>
    /// Creates a mutable CoreFoundation dictionary.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial nint CFDictionaryCreateMutable(
        nint allocator,
        nint capacity,
        nint keyCallBacks,
        nint valueCallBacks);

    /// <summary>
    /// Adds or replaces a value in a mutable CoreFoundation dictionary.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial void CFDictionarySetValue(
        nint dictionary,
        nint key,
        nint value);

    /// <summary>
    /// Creates a CFString from a UTF-8 string.
    /// </summary>
    [LibraryImport(
        Library,
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint CFStringCreateWithCString(
        nint allocator,
        string value,
        uint encoding);

    /// <summary>
    /// Creates a CFData object from raw bytes.
    /// </summary>
    [LibraryImport(Library)]
    internal static unsafe partial nint CFDataCreate(
        nint allocator,
        byte* bytes,
        nint length);

    /// <summary>
    /// Gets a pointer to the bytes stored inside a CFData object.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial nint CFDataGetBytePtr(
        nint data);

    /// <summary>
    /// Gets the number of bytes contained in a CFData object.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial nint CFDataGetLength(
        nint data);

    /// <summary>
    /// Releases a CoreFoundation object.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial void CFRelease(
        nint value);
    
    [LibraryImport(Library)]
    internal static partial nuint CFGetTypeID(
        nint cf);

    [LibraryImport(Library)]
    internal static partial nuint CFDataGetTypeID();

    [LibraryImport(Library)]
    internal static partial nuint CFDictionaryGetTypeID();
}