using Vogen;

namespace Chatly.WebApi.Common.Extensions;

public static class VogenIdValidationExtensions
{
    extension(Guid guid)
    {
        public Validation Validate<T>() where T : struct
        {
            return guid != Guid.Empty
                ? Validation.Ok
                : Validation.Invalid($"{typeof(T).Name} must be set to a non-default value.");
        }
    }
}