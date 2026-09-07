using System.Collections.Specialized;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Views.Pages.ChatPage;

[SingletonService(typeof(INavigableView<ChatPageViewModel>))]
public partial class ChatPage : UserControl, INavigableView<ChatPageViewModel>
{
    private bool _canLoadOlderMessages;
    private bool _isLoadingOlderMessages;

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
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _canLoadOlderMessages = false;
            return;
        }

        if (e.Action != NotifyCollectionChangedAction.Add ||
            e.NewStartingIndex + e.NewItems?.Count != ViewModel.Messages.Count)
        {
            return;
        }

        var distanceFromBottom = MessagesScrollViewer.Extent.Height -
                                 MessagesScrollViewer.Viewport.Height -
                                 MessagesScrollViewer.Offset.Y;
        if (distanceFromBottom <= 40)
        {
            Dispatcher.UIThread.Post(() =>
            {
                MessagesScrollViewer.ScrollToEnd();
                _canLoadOlderMessages = true;
            }, DispatcherPriority.Loaded);
        }
    }

    private async void MessagesScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (!_canLoadOlderMessages ||
            _isLoadingOlderMessages ||
            MessagesScrollViewer.Offset.Y > 48 ||
            !ViewModel.LoadOlderMessagesCommand.CanExecute(null))
        {
            return;
        }

        _isLoadingOlderMessages = true;
        var previousExtentHeight = MessagesScrollViewer.Extent.Height;
        var previousOffset = MessagesScrollViewer.Offset;

        try
        {
            await ViewModel.LoadOlderMessagesCommand.ExecuteAsync(null);
        }
        finally
        {
            Dispatcher.UIThread.Post(() =>
            {
                var addedHeight = MessagesScrollViewer.Extent.Height - previousExtentHeight;
                MessagesScrollViewer.Offset = new Vector(
                    previousOffset.X,
                    previousOffset.Y + Math.Max(0, addedHeight));
                _isLoadingOlderMessages = false;
            }, DispatcherPriority.Loaded);
        }
    }
}