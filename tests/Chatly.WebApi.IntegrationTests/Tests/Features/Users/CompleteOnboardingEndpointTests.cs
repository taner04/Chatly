using Chatly.WebApi.Common.Infrastructure.Email;
using NSubstitute;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class CompleteOnboardingEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task CompleteOnboarding_Should_SetUsernameAndSendWelcomeEmail_When_UserIsNew()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "identity|newcomer", "newcomer@chatly.tests",
            "newcomer");
        var client = CreateAuthenticatedClient(newUser);

        var response = await client.CompleteOnboardingAsync("newcomer", CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Username.Should().Be("newcomer");
        response.Content.OnboardingCompleted.Should().BeTrue();
        await EmailService.Received(1).SendEmailAsync(
            newUser.Email,
            Arg.Any<EmailRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteOnboarding_Should_Return409AndNotSendEmail_When_UsernameIsTaken()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "identity|late", "late@chatly.tests", "late");

        var response = await CreateAuthenticatedClient(newUser).CompleteOnboardingAsync(
            CurrentUser.Username,
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await EmailService.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, CurrentCancellationToken);
    }

    [Fact]
    public async Task CompleteOnboarding_Should_Return409AndNotSendEmailAgain_When_OnboardingIsCompleted()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "identity|twice", "twice@chatly.tests", "twice");
        var client = CreateAuthenticatedClient(newUser);
        await client.CompleteOnboardingAsync("twice", CurrentCancellationToken);

        var response = await client.CompleteOnboardingAsync("renamed", CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Conflict, "User.Onboarding.AlreadyCompleted");
        await EmailService.Received(1).SendEmailAsync(
            newUser.Email,
            Arg.Any<EmailRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteOnboarding_Should_CompleteOnboarding_When_WelcomeEmailFails()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "identity|smtp", "smtp@chatly.tests", "smtp");
        EmailService
            .SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("SMTP down")));

        var response = await CreateAuthenticatedClient(newUser).CompleteOnboardingAsync(
            "smtp",
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.OnboardingCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteOnboarding_Should_Return400_When_UsernameIsInvalid()
    {
        var newUser = new TestUser(UserId.From(Guid.NewGuid()), "identity|bad", "bad@chatly.tests", "bad");

        var response = await CreateAuthenticatedClient(newUser).CompleteOnboardingAsync(
            "no spaces allowed",
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}