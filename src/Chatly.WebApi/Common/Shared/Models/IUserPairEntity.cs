namespace Chatly.WebApi.Common.Shared.Models;

public interface IUserPairEntity
{
    UserId FirstUserId { get; }
    UserId SecondUserId { get; }
}