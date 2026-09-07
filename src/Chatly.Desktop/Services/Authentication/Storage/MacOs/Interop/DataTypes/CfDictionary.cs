namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop.DataTypes;

internal sealed class CfDictionary : Cf
{
    public CfDictionary()
    {
        Handle =
            CoreFoundationNative.CFDictionaryCreateMutable(
                0,
                0,
                0,
                0);
    }

    public void Set(
        nint key,
        nint value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(key);
        ArgumentOutOfRangeException.ThrowIfZero(value);

        CoreFoundationNative.CFDictionarySetValue(
            Handle,
            key,
            value);
    }
}