using System.Net;

namespace Chatly.WebApi.Features.DeviceSessions.Exceptions;

internal sealed class DeviceSessionHeaderInvalidException(string header)
    : ChatlyException(
        "Device session header invalid",
        $"The '{header}' header is missing or invalid.",
        "DeviceSession.HeaderInvalid",
        HttpStatusCode.BadRequest);