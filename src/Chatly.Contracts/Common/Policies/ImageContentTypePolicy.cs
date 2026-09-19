namespace Chatly.Contracts.Common.Policies;

public static class ImageContentTypePolicy
{
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string WebP = "image/webp";
    public const string Gif = "image/gif";

    public static IReadOnlySet<string> ProfilePictureContentTypes { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Jpeg, Png, WebP };

    public static IReadOnlySet<string> ImageContentTypes { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Jpeg, Png, WebP, Gif };

    public static IReadOnlyList<string> ProfilePictureFilePatterns { get; } =
        ["*.jpg", "*.jpeg", "*.png", "*.webp"];

    public static string? FromFileExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => Jpeg,
            ".png" => Png,
            ".webp" => WebP,
            ".gif" => Gif,
            _ => null
        };
    }
}