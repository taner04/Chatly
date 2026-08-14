using System.Threading.Tasks;

namespace Chatly.Desktop.Abstractions.Overlays;

public interface IPopupViewModel
{
    Task Completion { get; }
}

public interface IPopupViewModel<T>
{
    Task<T> Completion { get; }
}
