namespace Chatly.WebApi.Common.Shared.Models;

public abstract class Entity<TId> : Auditable where TId : struct
{
    protected Entity()
    {
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; private init; }
}