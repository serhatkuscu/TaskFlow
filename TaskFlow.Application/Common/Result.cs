namespace TaskFlow.Application.Common;

public sealed class Error
{
    public string Code { get; }
    public string Message { get; }

    private Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Create(string code, string message) => new(code, message);

    // Ortak hata kodları — tüm projede tutarlı kullanım için
    public static class Codes
    {
        public const string NotFound       = "NOT_FOUND";
        public const string Validation     = "VALIDATION_ERROR";
        public const string Conflict       = "CONFLICT";
        public const string Unauthorized   = "UNAUTHORIZED";
        public const string ServerError    = "SERVER_ERROR";
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result where T : notnull
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Başarısız sonuçta Value'ya erişilemez.");

    private Result(T value) : base(true, null)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);
}
