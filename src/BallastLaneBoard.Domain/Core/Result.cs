using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BallastLaneBoard.Domain.Core;

/// <summary>
/// Represents the result of an operation using the built-in <see cref="int"/> enum.
/// </summary>
[DebuggerDisplay("{IsSuccess ? \"Success\" : \"Failed (Error = \" + Error + \")\",nq}")]
public readonly struct Result
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailed => !IsSuccess;

    public int? Error { get; }

    internal Result(bool isSuccess, int? error)
    {
        IsSuccess = isSuccess;
        Error = isSuccess ? null : error;
    }

    public static Result Ok() => new(true, null);

    public static Result<T> Ok<T>(T value) => new(true, null, value);

    public static Result Fail() => new(false, 0);

    public static Result Fail(Enum error) => new(false, error.GetHashCode());

    public static Result<T> Fail<T>() => new(false, 0, default);

    public static Result<T> Fail<T>(Enum error) => new(false, error.GetHashCode(), default);

    public static Result<T> Fail<T>(int errorCode) => new(false, errorCode, default);
}

[DebuggerDisplay("{IsSuccess ? \"Success (Value = \" + Value + \")\" : \"Failed (Error = \" + Error + \")\",nq}")]
public readonly struct Result<T>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailed => !IsSuccess;

    public int? Error { get; }

    public T? Value { get; }

    internal Result(bool isSuccess, int? error, T? value)
    {
        IsSuccess = isSuccess;
        Error = isSuccess ? null : error;
        Value = value;
    }

    public static implicit operator Result(Result<T> result) => new(result.IsSuccess, result.Error);

    public static implicit operator Result<T>(Result result) => new(result.IsSuccess, result.Error, default);
}

public static class ResultExtensions
{
    public static bool TryGetValue<T>(this Result<T> result, [MaybeNullWhen(false)] out T? value)
    {
        value = result.IsSuccess ? result.Value : default;
        return result.IsSuccess;
    }
}
