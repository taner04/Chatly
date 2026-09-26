using Chatly.WebApi.Common.Infrastructure.Email;
using Chatly.WebApi.Common.Infrastructure.Hangfire.RecurringJobs.Absence;
using NSubstitute;

namespace Chatly.WebApi.IntegrationTests.Tests.Common.Hangfire;

public sealed class SendAbsenceEmailJobTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Execute_Should_SendEmailOnlyOnce_When_UserWasInactiveForAWeek()
    {
        await SetLastSeenAsync(CurrentUser, TimeSpan.FromDays(8));

        await RunJobAsync(CurrentUser);
        await RunJobAsync(CurrentUser);

        await EmailService.Received(1).SendEmailAsync(
            CurrentUser.Email,
            Arg.Any<EmailRequest>(),
            Arg.Any<CancellationToken>());
        await using var dbContext = GetDbContext();
        var user = await dbContext.Users.SingleAsync(user => user.Id == CurrentUser.Id, CurrentCancellationToken);
        Assert.NotNull(user.LastAbsenceEmailAt);
    }

    [Fact]
    public async Task Execute_Should_NotSendEmail_When_UserWasActiveRecently()
    {
        await SetLastSeenAsync(CurrentUser, TimeSpan.FromDays(2));

        await RunJobAsync(CurrentUser);

        await EmailService.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, CurrentCancellationToken);
    }

    [Fact]
    public async Task Execute_Should_AllowRetry_When_SendingEmailFails()
    {
        await SetLastSeenAsync(CurrentUser, TimeSpan.FromDays(8));
        EmailService
            .SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("SMTP down")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunJobAsync(CurrentUser));

        await using var dbContext = GetDbContext();
        var user = await dbContext.Users.SingleAsync(user => user.Id == CurrentUser.Id, CurrentCancellationToken);
        Assert.Null(user.LastAbsenceEmailAt);
    }

    private async Task SetLastSeenAsync(TestUser user, TimeSpan ago)
    {
        await using var dbContext = GetDbContext();
        await dbContext.Users
            .Where(candidate => candidate.Id == user.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(candidate => candidate.LastSeenAt, DateTimeOffset.UtcNow - ago),
                CurrentCancellationToken);
    }

    private async Task RunJobAsync(TestUser user)
    {
        using var scope = CreateScope();
        await scope.ServiceProvider.GetRequiredService<SendAbsenceEmailJob>().ExecuteAsync(user.Id, CurrentCancellationToken);
    }
}
