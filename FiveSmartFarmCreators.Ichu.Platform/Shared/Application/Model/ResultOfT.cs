using DomainErrorModel = FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors.Error;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

/// <summary>
///     Represents the outcome of an application operation: a value on success, or a domain error and the
///     context-specific error enum on failure.
/// </summary>
/// <typeparam name="T">The type of the value produced on success.</typeparam>
public class Result<T>
{
    /// <summary>
    ///     Initializes a new result.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="value">The value produced on success.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="errorEnum">The context-specific error enum of the failure.</param>
    /// <param name="error">The domain error of the failure.</param>
    protected Result(bool isSuccess, T? value, string message, Enum? errorEnum, DomainErrorModel? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
        Error = errorEnum;
        DomainError = error ?? (isSuccess ? DomainErrorModel.None : DomainErrorModel.Failure(errorEnum?.ToString() ?? "Error", message));
    }

    /// <summary>
    ///     Gets whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets the value produced on success.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    ///     Gets the failure message, or an empty text on success.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the context-specific error enum, which interfaces map to their own status codes.
    /// </summary>
    public Enum? Error { get; }

    /// <summary>
    ///     Gets the domain error, or <see cref="DomainErrorModel.None"/> on success.
    /// </summary>
    public DomainErrorModel DomainError { get; }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>The successful result.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty, null, DomainErrorModel.None);
    }

    /// <summary>
    ///     Creates a failed result from a context-specific error enum and a message.
    /// </summary>
    /// <param name="error">The context-specific error enum.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>The failed result.</returns>
    public static Result<T> Failure(Enum error, string message)
    {
        return new Result<T>(false, default, message, error, DomainErrorModel.Failure(error.ToString(), message));
    }

    /// <summary>
    ///     Creates a failed result from a domain error.
    /// </summary>
    /// <param name="error">The domain error.</param>
    /// <returns>The failed result.</returns>
    public static Result<T> Failure(DomainErrorModel error)
    {
        return new Result<T>(false, default, error.Message, null, error);
    }

    /// <summary>
    ///     Creates a failed result from a domain error and its context-specific error enum.
    /// </summary>
    /// <param name="error">The domain error.</param>
    /// <param name="errorEnum">The context-specific error enum.</param>
    /// <returns>The failed result.</returns>
    public static Result<T> Failure(DomainErrorModel error, Enum errorEnum)
    {
        return new Result<T>(false, default, error.Message, errorEnum, error);
    }

    /// <summary>
    ///     Produces a value from the success or the failure branch.
    /// </summary>
    /// <typeparam name="TResult">The type of the produced value.</typeparam>
    /// <param name="onSuccess">The function applied to the value on success.</param>
    /// <param name="onFailure">The function applied to the domain error on failure.</param>
    /// <returns>The value produced by the selected function.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainErrorModel, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(DomainError);
}
