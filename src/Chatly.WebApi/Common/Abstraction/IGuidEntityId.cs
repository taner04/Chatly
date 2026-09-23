namespace Chatly.WebApi.Common.Abstraction;

public interface IGuidEntityId<out TSelf> where TSelf : struct, IGuidEntityId<TSelf>
{
    static abstract TSelf From(Guid value);
}
