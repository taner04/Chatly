namespace Chatly.WebApi.Common.Abstraction;

public interface IEndpoint
{
    void MapEndpoint(WebApplication app);
}