using Chatly.WebApi.Common.Infrastructure.Email;

namespace Chatly.WebApi.Common.Infrastructure.Hangfire.RecurringJobs.Absence;

[ScopedService]
internal sealed class SendAbsenceEmailJob(
    ChatlyDbContext context,
    EmailService emailService)
{
    public async Task ExecuteAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTimeOffset.UtcNow.AddDays(-7);

        var user = await context.Users
            .AsNoTracking()
            .Where(AbsenceDbQueryFilter.IsInactiveSince(thresholdDate))
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Email,
                u.Username,
                u.LastSeenAt,
                u.LastAbsenceEmailAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return;
        }

        var claimedAt = DateTimeOffset.UtcNow;
        var claimed = await context.Users
            .Where(u => u.Id == userId)
            .Where(AbsenceDbQueryFilter.IsInactiveSince(thresholdDate))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    u => u.LastAbsenceEmailAt,
                    claimedAt),
                cancellationToken);

        if (claimed == 0)
        {
            return;
        }

        var emailRequest = new EmailRequest(
                "We miss you at Chatly!",
                EmailTemplateKeys.Absence)
            .AddValue("Username", user.Username!)
            .AddValue(
                "LastSeenAt",
                user.LastSeenAt?.ToString("dd.MM.yyyy") ?? "Unknown");

        try
        {
            await emailService.SendEmailAsync(
                user.Email,
                emailRequest,
                cancellationToken);
        }
        catch
        {
            await context.Users
                .Where(u => u.Id == userId && u.LastAbsenceEmailAt == claimedAt)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        u => u.LastAbsenceEmailAt,
                        user.LastAbsenceEmailAt),
                    CancellationToken.None);
            throw;
        }
    }
}
