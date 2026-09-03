using System.Collections.Specialized;
using Avalonia.Input;
using Avalonia.Threading;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Views.Pages.ChatPage;

[SingletonService(typeof(INavigableView<ChatPageViewModel>))]
public partial class ChatPage : UserControl, INavigableView<ChatPageViewModel>
{
    public ChatPage(ChatPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
    }

    public ChatPageViewModel ViewModel { get; }

    private void MessageTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || !ViewModel.SendMessageCommand.CanExecute(null))
        {
            return;
        }

        e.Handled = true;
        ViewModel.SendMessageCommand.Execute(null);
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add ||
            e.NewStartingIndex + e.NewItems?.Count != ViewModel.Messages.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(MessagesScrollViewer.ScrollToEnd, DispatcherPriority.Loaded);
    }
}