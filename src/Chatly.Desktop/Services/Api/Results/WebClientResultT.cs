namespace Chatly.Desktop.Services.Api.Results;

internal sealed class WebClientResult<T> : WebClientResult
{
    private WebClientResult(T value) : base(null!)
    {
        Value = value;
    }

    private WebClientResult(WebClientError error) : base(error)
    {
    }


    public T Value => field ??
                      throw new InvalidOperationException("Cannot access Value when the response is not successful.");

    public static implicit operator WebClientResult<T>(T value)
    {
        return new WebClientResult<T>(value);
    }

    public static implicit operator WebClientResult<T>(WebClientError error)
    {
        return new WebClientResult<T>(error);
    }
}
