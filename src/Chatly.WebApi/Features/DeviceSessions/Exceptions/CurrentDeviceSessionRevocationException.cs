using System.Net;

namespace Chatly.WebApi.Features.DeviceSessions.Exceptions;

internal sealed class CurrentDeviceSessionRevocationException()
    : ChatlyException(
        "Current device session cannot be revoked",
        "Use log out to end the session of the current device.",
        "DeviceSession.Current",
        HttpStatusCode.Conflict);