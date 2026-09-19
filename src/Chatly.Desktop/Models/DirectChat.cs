namespace Chatly.Desktop.Models;

public sealed partial class DirectChat : ObservableObject, IIdentifiable
{
    public required User User { get; init; }

    [ObservableProperty] public partial int UnreadMessageCount { get; set; }
    public required Guid Id { get; init; }
}