namespace Chatly.Desktop.Services.Api.Results;

public class WebClientResult
{
    private readonly WebClientError? _error;

    protected WebClientResult(WebClientError? error)
    {
        _error = error;
    }

    public bool IsSuccess => _error is null;
    public bool IsFailure => !IsSuccess;

    public WebClientError Error => _error ??
                                   throw new InvalidOperationException(
                                       "Cannot access ProblemDetails when the response is successful.");

    public static implicit operator WebClientResult(WebClientError error)
    {
        return new WebClientResult(error);
    }

    public static WebClientResult Success()
    {
        return new WebClientResult(null);
    }

    public static WebClientResult Failure(WebClientError error)
    {
        return new WebClientResult(error);
    }
}