using Chatly.Contracts.Features.Hubs;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs.TestDoubles;

internal sealed record TestCall(Guid Id)
    : CallMessage(Id, Guid.NewGuid(), "remote", CallRole.Caller, CallState.Ringing);
