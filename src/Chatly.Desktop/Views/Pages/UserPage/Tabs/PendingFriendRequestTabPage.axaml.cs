using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

[SingletonService(typeof(INavigableView<PendingFriendRequestTabPageViewModel>))]
public partial class PendingFriendRequestTabPage
    : UserControl, INavigableView<PendingFriendRequestTabPageViewModel>
{
    private bool _isLoadingMoreFriendRequests;

    public PendingFriendRequestTabPage(PendingFriendRequestTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public PendingFriendRequestTabPageViewModel ViewModel { get; }

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