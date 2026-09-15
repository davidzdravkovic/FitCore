namespace FitCore.Api.Errors.Business;

public sealed class Result
{
    public string? Error { get; }
    public object? Details { get; }
    public bool Succeeded => Error is null;

    private Result(string? error, object? details = null)
    {
        Error = error;
        Details = details;
    }

    public static Result Success() => new(null);
    public static Result Fail(string error, object? details = null) => new(error, details);
}

public sealed class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool Succeeded => Error is null;

    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Fail(string error) => new(default, error);
}
