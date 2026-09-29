using System.Collections.Specialized;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Views.Pages.ChatPage;

internal partial class ChatPage : UserControl, IViewFor<ChatPageViewModel>
{
    private bool _canLoadOlderMessages;
    private bool _isLoadingOlderMessages;
    private bool _shouldFollowLatestMessage;

    public ChatPage(ChatPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        InitializeComponent();

        ViewModel.MessagesViewModel.Messages.CollectionChanged += Messages_CollectionChanged;

        var dropTargetGrid = this.FindControl<Grid>("DropTargetGrid");
        var dropTargetTextBox = this.FindControl<TextBox>("DropTargetTextBox");
        ArgumentNullException.ThrowIfNull(dropTargetGrid);
        ArgumentNullException.ThrowIfNull(dropTargetTextBox);

        DragDrop.AddDragOverHandler(dropTargetGrid, OnDragOver);
        DragDrop.AddDragOverHandler(dropTargetTextBox, OnDragOver);

        DragDrop.AddDropHandler(dropTargetGrid, OnDrop);
        DragDrop.AddDropHandler(dropTargetTextBox, OnDrop);
    }

    public ChatPageViewModel ViewModel { get; }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        var containsFiles = e.DataTransfer.TryGetFiles()?.Any(item => item is IStorageFile) == true;
        e.DragEffects = ViewModel.HasCurrentChat && containsFiles
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;

        if (!ViewModel.HasCurrentChat)
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles()?.OfType<IStorageFile>().ToArray();
        if (files is { Length: > 0 })
        {
            await ViewModel.AddAttachmentAsync(files);
        }
    }

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
            _shouldFollowLatestMessage = false;
            return;
        }

        if (e.Action != NotifyCollectionChangedAction.Add ||
            e.NewStartingIndex + e.NewItems?.Count != ViewModel.MessagesViewModel.Messages.Count)
        {
            return;
        }

        var distanceFromBottom = MessagesScrollViewer.Extent.Height -
                                 MessagesScrollViewer.Viewport.Height -
                                 MessagesScrollViewer.Offset.Y;

        if (!(distanceFromBottom <= 40))
        {
            return;
        }

        _shouldFollowLatestMessage = true;
        Dispatcher.UIThread.Post(() =>
        {
            MessagesScrollViewer.ScrollToEnd();
            _canLoadOlderMessages = true;
        }, DispatcherPriority.Loaded);
    }

    private async void MessagesScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_shouldFollowLatestMessage &&
            !_isLoadingOlderMessages &&
            e.ExtentDelta.Y > 0)
        {
            Dispatcher.UIThread.Post(
                MessagesScrollViewer.ScrollToEnd,
                DispatcherPriority.Loaded);
        }

        if (e.OffsetDelta.Y < 0 && e.ExtentDelta.Y == 0)
        {
            _shouldFollowLatestMessage = false;
        }
        else
        {
            var distanceFromBottom = MessagesScrollViewer.Extent.Height -
                                     MessagesScrollViewer.Viewport.Height -
                                     MessagesScrollViewer.Offset.Y;
            if (distanceFromBottom <= 40)
            {
                _shouldFollowLatestMessage = true;
            }
        }

        if (!_canLoadOlderMessages ||
            _isLoadingOlderMessages ||
            MessagesScrollViewer.Offset.Y > 48 ||
            !ViewModel.MessagesViewModel.LoadOlderMessagesCommand.CanExecute(null))
        {
            return;
        }

        _isLoadingOlderMessages = true;
        var previousExtentHeight = MessagesScrollViewer.Extent.Height;
        var previousOffset = MessagesScrollViewer.Offset;

        try
        {
            await ViewModel.MessagesViewModel.LoadOlderMessagesCommand.ExecuteAsync(null);
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