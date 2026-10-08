using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.ViewModels.Windows;

namespace Chatly.Desktop.UnitTests.Tests.ViewModels.Windows;

public sealed class SplashScreenViewModelTests : TestBase, IDisposable
{
    private readonly SplashScreenViewModel _viewModel = new();

    public void Dispose() => _viewModel.Dispose();

    [Fact]
    public async Task WaitForRetryAsync_Should_ShowErrorUntilRetry_When_SignInFailed()
    {
        var waiting = _viewModel.WaitForRetryAsync("Signing in failed.", CurrentCancellationToken);

        _viewModel.HasError.Should().BeTrue();
        _viewModel.StartupMessage.Should().Be("Signing in failed.");
        waiting.IsCompleted.Should().BeFalse();

        _viewModel.RetryCommand.Execute(null);
        await waiting;

        _viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task WaitForRetryAsync_Should_ThrowCancellation_When_UserCancels()
    {
        var waiting = _viewModel.WaitForRetryAsync("Signing in failed.", _viewModel.CancellationToken);

        _viewModel.CancelCommand.Execute(null);

        await FluentActions.Awaiting(() => waiting).Should().ThrowAsync<OperationCanceledException>();
        _viewModel.HasError.Should().BeFalse();
    }
}