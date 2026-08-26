using System.Threading.Tasks;
using Chatly.Contracts.SignalR;

namespace Chatly.Desktop.Services.Api.SignalR;

public interface INotificationStrategy
{
    NotificationType Type { get; }

    Task HandleNotificationAsync(NotificationMessage message);
}