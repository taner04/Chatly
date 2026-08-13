using System.Threading.Tasks;

namespace Chatly.Desktop.Shared.Abstractions;

public interface IMessageOverlay
{
    Task Completion { get; }
}
