namespace EkubCircle.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, string message, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? new List<string>();
    }

    public static Result Success(string message = "Operation succeeded.") => new(true, message);
    public static Result Failure(string message, List<string>? errors = null) => new(false, message, errors);
}

public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool isSuccess, T? data, string message, List<string>? errors = null) 
        : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string message = "Operation succeeded.") => new(true, data, message);
    public static new Result<T> Failure(string message, List<string>? errors = null) => new(false, default, message, errors);
}
