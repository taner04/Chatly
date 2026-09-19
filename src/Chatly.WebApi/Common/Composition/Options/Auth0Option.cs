using Chatly.Shared.Attributes;
using Chatly.Shared.Options;

namespace Chatly.WebApi.Common.Composition.Options;

[Option]
internal sealed class Auth0Option : Auth0OptionBase
{
    public bool UsePersistentStorage { get; init; }
}