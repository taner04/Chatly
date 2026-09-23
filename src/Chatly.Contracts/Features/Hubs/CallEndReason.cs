namespace Chatly.Contracts.Features.Hubs;

public enum CallEndReason
{
    Completed,
    Declined,
    Cancelled,
    Missed,
    Busy,
    Failed
}
