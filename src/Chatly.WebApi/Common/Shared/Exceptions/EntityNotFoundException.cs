using System.Net;

namespace Chatly.WebApi.Common.Shared.Exceptions;

public sealed class EntityNotFoundException<T>(Guid id) :
    ChatlyException(
        $"Could not find {typeof(T).Name.ToLower()}.",
        $"{typeof(T).Name} with ID '{id}' was not found.",
        $"{typeof(T).Name}.NotFound",
        HttpStatusCode.NotFound);