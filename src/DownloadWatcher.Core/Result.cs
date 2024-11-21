namespace DownloadWatcher.Core;

public class Result<T>
{
    private readonly T? _value;
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "Cannot access Value when the result is a failure."
            );
    public List<string> Errors { get; } = [];

    public bool IsSuccess => Errors.Count == 0;

    private Result(T value)
    {
        _value = value;
    }

    private Result(IEnumerable<string> errors)
    {
        Errors.AddRange(errors);
    }

    public static Result<T> Success(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException($"{nameof(value)} cannot be null");
        }

        return new(value);
    }

    public static Result<T> Failure(IEnumerable<string> errors) => new(errors);
}
