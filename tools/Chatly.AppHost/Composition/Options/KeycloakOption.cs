using System.ComponentModel.DataAnnotations;

namespace Chatly.AppHost.Composition.Options;

internal sealed class KeycloakOption
{
    [Required(ErrorMessage = "Keycloak image registry is required.")]
    public string Registry { get; init; } = null!;

    [Required(ErrorMessage = "Keycloak image is required.")]
    public string Image { get; init; } = null!;

    [Required(ErrorMessage = "Keycloak image tag is required.")]
    public string Tag { get; init; } = null!;

    [Range(1024, 65535, ErrorMessage = "Keycloak port must be between 1024 and 65535.")]
    public int Port { get; init; }

    [Required(ErrorMessage = "Keycloak realm import path is required.")]
    public string RealmImportPath { get; init; } = null!;

    [Required(ErrorMessage = "Keycloak themes path is required.")]
    public string ThemesPath { get; init; } = null!;

    public bool PersistData { get; init; }
}