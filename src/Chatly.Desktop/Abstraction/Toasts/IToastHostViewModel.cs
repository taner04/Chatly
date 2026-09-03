using System.Collections.ObjectModel;

namespace Chatly.Desktop.Abstraction.Toasts;

public interface IToastHostViewModel
{
    ObservableCollection<IToastViewModel> Toasts { get; }
}