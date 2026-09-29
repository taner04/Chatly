using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

internal partial class PendingFriendRequestsTabPage
    : UserControl, IViewFor<PendingFriendRequestsTabPageViewModel>
{
    private bool _isLoadingMoreFriendRequests;

    public PendingFriendRequestsTabPage(PendingFriendRequestsTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public PendingFriendRequestsTabPageViewModel ViewModel { get; }

    private async void FriendRequestsScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var distanceFromBottom = FriendRequestsScrollViewer.Extent.Height
                                 - FriendRequestsScrollViewer.Viewport.Height
                                 - FriendRequestsScrollViewer.Offset.Y;

        if (_isLoadingMoreFriendRequests ||
            ViewModel.IsLoadingFriendRequests ||
            !ViewModel.HasMoreFriendRequests ||
            distanceFromBottom > 120)
        {
            return;
        }

        _isLoadingMoreFriendRequests = true;
        try
        {
            await ViewModel.LoadMoreFriendRequestsCommand.ExecuteAsync(null);
        }
        finally
        {
            _isLoadingMoreFriendRequests = false;
        }
    }
}