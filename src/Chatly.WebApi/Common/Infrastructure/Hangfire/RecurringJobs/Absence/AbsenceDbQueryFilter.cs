using System.Linq.Expressions;

namespace Chatly.WebApi.Common.Infrastructure.Hangfire.RecurringJobs.Absence;

internal static class AbsenceDbQueryFilter
{
    internal static Expression<Func<User, bool>> IsInactiveSince(
        DateTimeOffset thresholdDate)
    {
        return user =>
            user.LastSeenAt < thresholdDate &&
            user.OnboardingCompleted &&
            (user.LastAbsenceEmailAt == null ||
             user.LastAbsenceEmailAt < user.LastSeenAt);
    }
}