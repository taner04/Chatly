using System.Net;

namespace Chatly.WebApi.Common.Shared.Exceptions;

public sealed class EntityNotFoundException<T>(Guid id)
    : ChatlyException(
        $"{typeof(T).Name} not found",
        $"No {typeof(T).Name} with ID '{id}' was found.",
        $"{typeof(T).Name}.NotFound",
        HttpStatusCode.NotFound);
