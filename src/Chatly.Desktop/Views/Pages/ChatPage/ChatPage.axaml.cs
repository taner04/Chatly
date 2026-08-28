using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Views.Pages.ChatPage;

public partial class ChatPage : UserControl, INavigableView<ChatPageViewModel>
{
    public ChatPage(ChatPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public ChatPageViewModel ViewModel { get; }
}
