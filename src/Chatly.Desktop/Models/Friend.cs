namespace Chatly.Desktop.Models;

public sealed class Friend
{
    public required User User { get; init; }
    public Guid? ChatId { get; set; }
}