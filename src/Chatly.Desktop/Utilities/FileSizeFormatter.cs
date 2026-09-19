namespace Chatly.Desktop.Utilities;

internal static class FileSizeFormatter
{
    internal static string Format(long size)
    {
        return size switch
        {
            >= 1024 * 1024 => $"{size / (1024d * 1024d):0.#} MB",
            >= 1024 => $"{size / 1024d:0.#} KB",
            _ => $"{size} B"
        };
    }
}