using System.Net;

namespace Chatly.WebApi.Features.DeviceSessions.Exceptions;

internal sealed class DeviceSessionRevokedException()
    : ChatlyException(
        "Device session revoked",
        "This device was signed out. Sign in again to continue.",
        DeviceSessionErrorCodes.Revoked,
        HttpStatusCode.Unauthorized);