using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;

namespace Chatly.Desktop.Options;

[Option]
public sealed class DesktopProfileOption
{
    public const string DefaultName = "default";

    [Required(ErrorMessage = "Desktop profile name is required.")]
    [RegularExpression(
        "^[A-Za-z0-9_-]+$",
        ErrorMessage = "Desktop profile name may contain only letters, numbers, hyphens, and underscores.")]
    public string Name { get; init; } = DefaultName;
}