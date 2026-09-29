using Chatly.Desktop.Services.Toasts;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.ViewModels.Toasts;
using FluentIcons.Common;

namespace Chatly.Desktop.UnitTests.Tests.Services.Toasts;

public sealed class ToastServiceTests
{
    private readonly ToastHostOverlayViewModel _toastHost = new();
    private readonly ToastService _toastService;

    public ToastServiceTests()
    {
        _toastService = new ToastService(_toastHost);
    }

    [Fact]
    public Task AddToast_Should_ShowNewestToastFirst() => UiThread.RunAsync(() =>
    {
        var first = CreateToast();
        var second = CreateToast();

        _toastService.AddToast(first);
        _toastService.AddToast(second);

        _toastHost.Toasts.Should().Equal(second, first);
        return Task.CompletedTask;
    });

    [Fact]
    public Task AddToast_Should_RemoveOldestToast_When_LimitIsReached() => UiThread.RunAsync(() =>
    {
        var toasts = Enumerable.Range(0, 6).Select(_ => CreateToast()).ToList();

        toasts.ForEach(_toastService.AddToast);

        _toastHost.Toasts.Should().HaveCount(5).And.NotContain(toasts[0]);
        return Task.CompletedTask;
    });

    [Fact]
    public Task Dismiss_Should_RemoveToast() => UiThread.RunAsync(() =>
    {
        var toast = CreateToast();
        _toastService.AddToast(toast);

        toast.DismissCommand.Execute(null);

        _toastHost.Toasts.Should().BeEmpty();
        return Task.CompletedTask;
    });

    private static ToastViewModel CreateToast() => new("Title", "Message", Symbol.Info);
}