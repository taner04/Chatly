namespace Chatly.WebApi.Common.Extensions;

internal static class VogenIdExtensions
{
    extension(Guid guid)
    {
        public Validation Validate<T>() where T : struct =>
            guid != Guid.Empty
                ? Validation.Ok
                : Validation.Invalid($"{typeof(T).Name} must be set to a non-default value.");
    }
}