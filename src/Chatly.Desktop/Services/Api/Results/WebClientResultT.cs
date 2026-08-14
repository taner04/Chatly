using System;

namespace Chatly.Desktop.Services.Api.Results;

public sealed class WebClientResult<T> : WebClientResult
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

    public static implicit operator WebClientResult<T>(T value) => new(value);

    public static implicit operator WebClientResult<T>(WebClientError error) => new(error);

    public static WebClientResult<T> Success(T value) => new(value);
}
