using Chatly.Desktop.Abstraction.Navigation;

namespace Chatly.Desktop.UnitTests.Tests.Services.Navigation.TestDoubles;

internal abstract class TestPage : INavigableViewModel
{
    internal List<string> Calls { get; } = [];

    internal object? Parameter { get; private set; }

    internal Func<CancellationToken, Task>? Entering { get; set; }

    public Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken)
    {
        Calls.Add("to");
        Parameter = parameter;
        return Entering?.Invoke(cancellationToken) ?? Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync(CancellationToken cancellationToken)
    {
        Calls.Add("from");
        return Task.CompletedTask;
    }
}

internal sealed class FirstPage : TestPage;

internal sealed class SecondPage : TestPage;