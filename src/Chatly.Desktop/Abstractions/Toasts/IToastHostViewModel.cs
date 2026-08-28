using System.Collections.ObjectModel;

namespace Chatly.Desktop.Abstractions.Toasts;

public interface IToastHostViewModel
{
    ObservableCollection<IToastViewModel> Toasts { get; }
}
