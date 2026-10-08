using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.ViewModels.Popups;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.UnitTests.Tests.Services.Popups;

public sealed class PopupServiceTests : TestBase
{
    private readonly PopupOverlayHostViewModel _popupHost = new();
    private readonly PopupService _popupService;

    public PopupServiceTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddTransient<TestPopup>()
            .BuildServiceProvider();
        _popupService = new PopupService(serviceProvider, _popupHost);
    }

    [Fact]
    public async Task ShowAsync_Should_ShowPopupUntilClosed()
    {
        var showing = _popupService.ShowAsync(typeof(TestPopup), CurrentCancellationToken);

        var popup = _popupHost.Current.Should().BeOfType<TestPopup>().Subject;
        _popupHost.IsOpen.Should().BeTrue();
        showing.IsCompleted.Should().BeFalse();

        popup.CloseOverlay();
        await showing;

        _popupHost.Current.Should().BeNull();
        _popupHost.IsOpen.Should().BeFalse();
    }

    [Fact]
    public async Task ShowAsync_Should_KeepCurrentPopup_When_PopupIsAlreadyOpen()
    {
        var showing = _popupService.ShowAsync(typeof(TestPopup), CurrentCancellationToken);
        var popup = _popupHost.Current;

        await _popupService.ShowAsync(typeof(TestPopup), CurrentCancellationToken);

        _popupHost.Current.Should().BeSameAs(popup);
        popup!.CloseOverlay();
        await showing;
    }

    [Fact]
    public async Task ShowAsync_Should_ShowGivenPopupUntilClosed_When_PopupInstanceIsPassed()
    {
        var popup = new ConfirmationPopupViewModel("Title", "Message", "OK", null);

        var showing = _popupService.ShowAsync(popup, CurrentCancellationToken);

        _popupHost.Current.Should().BeSameAs(popup);
        popup.ConfirmCommand.Execute(null);
        await showing;

        popup.IsConfirmed.Should().BeTrue();
        _popupHost.Current.Should().BeNull();
    }

    private sealed class TestPopup : PopupOverlayViewModel
    {
        public override string Title => "Test";
    }
}