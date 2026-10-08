using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Features.MessageAttachments.Models;

[ValueObject<Guid>]
public readonly partial struct MessageAttachmentId : IGuidEntityId<MessageAttachmentId>
{
    private static Validation Validate(Guid value) => value.Validate<MessageAttachmentId>();
}

public sealed class MessageAttachment : Entity<MessageAttachmentId>
{
    [UsedImplicitly]
    private MessageAttachment()
    {
    }

    public MessageAttachment(MessageId messageId, StoredFileId storedFileId)
    {
        MessageId = messageId;
        StoredFileId = storedFileId;
    }

    public MessageId MessageId { get; private init; }

    public StoredFileId StoredFileId { get; private init; }

    public StoredFile StoredFile { get; private init; } = null!;
}