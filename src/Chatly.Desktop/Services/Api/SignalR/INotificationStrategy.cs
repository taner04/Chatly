using Chatly.Contracts.SignalR;
using System.Threading.Tasks;

namespace Chatly.Desktop.Services.Api.SignalR;

public interface INotificationStrategy
{
    NotificationType Type { get; }
    
    Task HandleNotificationAsync(NotificationMessage message);
}