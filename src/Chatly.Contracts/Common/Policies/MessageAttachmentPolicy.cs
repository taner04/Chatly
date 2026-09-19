namespace Chatly.Contracts.Common.Policies;

public static class MessageAttachmentPolicy
{
    public const int MaxPerMessage = 5;
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;
    public const long MaxTotalSizePerMessageBytes = 25 * 1024 * 1024;
}