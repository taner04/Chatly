namespace Chatly.WebApi.Common.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(WebApplication app);
}