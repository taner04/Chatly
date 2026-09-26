namespace Chatly.WebApi.Common.Shared.Models;

public abstract class Entity<TId> : Auditable
    where TId : struct, IGuidEntityId<TId>
{
    public TId Id { get; private init; } = TId.From(Guid.CreateVersion7());
}