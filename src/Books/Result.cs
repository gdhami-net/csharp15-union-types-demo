namespace Books;

// The kind of Result class a C# codebase grows before it has unions.
public sealed class Result<T, TError>
{
    private readonly T? _value;
    private readonly TError? _error;

    public bool IsSuccess { get; }

    private Result(T value, TError? error, bool isSuccess)
    {
        _value = value;
        _error = error;
        IsSuccess = isSuccess;
    }

    public static Result<T, TError> Ok(T value) => new(value, default, true);
    public static Result<T, TError> Fail(TError error) => new(default!, error, false);

    public TOut Match<TOut>(Func<T, TOut> ok, Func<TError, TOut> fail) =>
        IsSuccess ? ok(_value!) : fail(_error!);
}

public abstract record LookupError;
public record BookNotFound(string Isbn) : LookupError;
public record InvalidIsbn(string Reason) : LookupError;
