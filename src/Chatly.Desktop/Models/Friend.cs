namespace Chatly.Desktop.Models;

public sealed class Friend : IIdentifiable
{
    public Guid Id => User.Id;

    public required User User { get; init; }
    public Guid? ChatId { get; set; }
}
