using Chatly.Desktop.Abstraction.Navigation;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Services.Navigation;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.UnitTests.Tests.Services.Navigation.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Navigation;

public sealed class NavigationServiceTests : TestBase, IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly FirstPage _first = new();
    private readonly List<INavigableViewModel> _navigated = [];
    private readonly NavigationService _navigationService;
    private readonly SecondPage _second = new();

    public NavigationServiceTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(_first)
            .AddSingleton(_second)
            .BuildServiceProvider();
        _navigationService = new NavigationService(serviceProvider, NullLogger<NavigationService>.Instance);
        _navigationService.Navigated += (_, e) => _navigated.Add(e.Page);
    }

    public void Dispose() => _cancellation.Dispose();

    [Fact]
    public async Task NavigateToAsync_Should_EnterPageAndRaiseNavigated_When_PageIsNew()
    {
        var navigated = await _navigationService.NavigateToAsync<FirstPage>(42, CurrentCancellationToken);

        navigated.Should().BeTrue();
        _navigationService.CurrentPage.Should().BeSameAs(_first);
        _first.Calls.Should().Equal("to");
        _first.Parameter.Should().Be(42);
        _navigated.Should().Equal(_first);
    }

    [Fact]
    public async Task NavigateToAsync_Should_ReturnFalse_When_AlreadyOnPage()
    {
        await _navigationService.NavigateToAsync<FirstPage>(CurrentCancellationToken);

        var navigated = await _navigationService.NavigateToAsync<FirstPage>(CurrentCancellationToken);

        navigated.Should().BeFalse();
        _first.Calls.Should().Equal("to");
    }

    [Fact]
    public async Task GoBackAndGoForward_Should_MoveThroughHistory()
    {
        await _navigationService.NavigateToAsync<FirstPage>(CurrentCancellationToken);
        await _navigationService.NavigateToAsync<SecondPage>(CurrentCancellationToken);

        (await _navigationService.GoBackAsync(CurrentCancellationToken)).Should().BeTrue();
        _navigationService.CurrentPage.Should().BeSameAs(_first);

        (await _navigationService.GoForwardAsync(CurrentCancellationToken)).Should().BeTrue();
        _navigationService.CurrentPage.Should().BeSameAs(_second);
        (await _navigationService.GoForwardAsync(CurrentCancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_Should_ClearForwardHistory_When_NavigatingToNewPage()
    {
        await _navigationService.NavigateToAsync<FirstPage>(CurrentCancellationToken);
        await _navigationService.NavigateToAsync<SecondPage>(CurrentCancellationToken);
        await _navigationService.GoBackAsync(CurrentCancellationToken);

        await _navigationService.NavigateToAsync<FirstPage>(1, CurrentCancellationToken);

        (await _navigationService.GoForwardAsync(CurrentCancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_Should_ReplaceCurrentEntry_When_SamePageWithNewParameter()
    {
        await _navigationService.NavigateToAsync<FirstPage>(1, CurrentCancellationToken);

        var navigated = await _navigationService.NavigateToAsync<FirstPage>(2, CurrentCancellationToken);

        navigated.Should().BeTrue();
        _first.Parameter.Should().Be(2);
        (await _navigationService.GoBackAsync(CurrentCancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_Should_LeaveTargetAndRestorePrevious_When_TargetFails()
    {
        await _navigationService.NavigateToAsync<FirstPage>(1, CurrentCancellationToken);
        _second.Entering = _ => Task.FromException(new InvalidOperationException());

        var navigated = await _navigationService.NavigateToAsync<SecondPage>(CurrentCancellationToken);

        navigated.Should().BeFalse();
        _navigationService.CurrentPage.Should().BeSameAs(_first);
        _second.Calls.Should().Equal("to", "from");
        _first.Calls.Should().Equal("to", "from", "to");
        _first.Parameter.Should().Be(1);
        _navigated.Should().Equal(_first);
        (await _navigationService.GoBackAsync(CurrentCancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_Should_RestorePreviousAndThrow_When_Canceled()
    {
        await _navigationService.NavigateToAsync<FirstPage>(CurrentCancellationToken);
        _second.Entering = _ =>
        {
            _cancellation.Cancel();
            return Task.FromCanceled(_cancellation.Token);
        };

        var navigate = () => _navigationService.NavigateToAsync<SecondPage>(_cancellation.Token);

        await navigate.Should().ThrowAsync<OperationCanceledException>();
        _navigationService.CurrentPage.Should().BeSameAs(_first);
        _first.Calls.Should().Equal("to", "from", "to");
    }

    [Fact]
    public async Task NavigateToAsync_Should_KeepHistoryConsistent_When_NavigatedHandlerThrows()
    {
        await _navigationService.NavigateToAsync<FirstPage>(CurrentCancellationToken);
        EventHandler<NavigatedEventArgs> throwingHandler = (_, _) => throw new InvalidOperationException();
        _navigationService.Navigated += throwingHandler;

        var navigate = () => _navigationService.NavigateToAsync<SecondPage>(CurrentCancellationToken);

        await navigate.Should().ThrowAsync<InvalidOperationException>();
        _navigationService.Navigated -= throwingHandler;
        _navigationService.CurrentPage.Should().BeSameAs(_second);
        (await _navigationService.GoBackAsync(CurrentCancellationToken)).Should().BeTrue();
        _navigationService.CurrentPage.Should().BeSameAs(_first);
    }
}