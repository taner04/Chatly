using System.Net;

namespace Chatly.WebApi.Features.Users.Exceptions;

internal sealed class OnboardingAlreadyCompletedException()
    : ChatlyException(
        "Onboarding already completed",
        "The onboarding has already been completed.",
        "User.Onboarding.AlreadyCompleted",
        HttpStatusCode.Conflict);