namespace Chatly.Desktop.Models;

public sealed class Friend : IIdentifiable
{
    public required User User { get; init; }
    public Guid? ChatId { get; set; }
    public Guid Id => User.Id;
}