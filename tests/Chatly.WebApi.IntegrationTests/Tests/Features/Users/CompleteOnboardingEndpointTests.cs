using Chatly.WebApi.Common.Infrastructure.Email;
using NSubstitute;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class CompleteOnboardingEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task CompleteOnboarding_Should_SetUsernameAndSendWelcomeEmail_When_UserIsNew()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "auth0|newcomer", "newcomer@chatly.tests", "newcomer");
        var client = CreateAuthenticatedClient(newUser);

        var response = await client.CompleteOnboardingAsync("newcomer", CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("newcomer", response.Content!.Username);
        Assert.True(response.Content.OnboardingCompleted);
        await EmailService.Received(1).SendEmailAsync(
            newUser.Email,
            Arg.Any<EmailRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteOnboarding_Should_Return409AndNotSendEmail_When_UsernameIsTaken()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "auth0|late", "late@chatly.tests", "late");

        var response = await CreateAuthenticatedClient(newUser).CompleteOnboardingAsync(
            CurrentUser.Username,
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await EmailService.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, CurrentCancellationToken);
    }

    [Fact]
    public async Task CompleteOnboarding_Should_Return400_When_UsernameIsInvalid()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "auth0|bad", "bad@chatly.tests", "bad");

        var response = await CreateAuthenticatedClient(newUser).CompleteOnboardingAsync(
            "no spaces allowed",
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}