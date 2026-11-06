using DomainErrorModel = FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors.Error;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

// Move to Shared when the shared kernel is available.
/// <summary>
///     Represents the outcome of an application operation: a value on success, or a domain error with the error code
///     of the bounded context on failure.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class Result<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="value">The value produced by a successful operation.</param>
    /// <param name="errorEnum">The error code of the bounded context, for a failed operation.</param>
    /// <param name="error">The domain error that explains a failed operation.</param>
    protected Result(bool isSuccess, T? value, Enum? errorEnum, DomainErrorModel error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = error.Message;
        Error = errorEnum;
        DomainError = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the value produced by a successful operation.</summary>
    public T? Value { get; }

    /// <summary>Gets the message of the domain error, or an empty string on success.</summary>
    public string Message { get; }

    /// <summary>Gets the error code of the bounded context, or <see langword="null"/> on success.</summary>
    public Enum? Error { get; }

    /// <summary>Gets the domain error, or <see cref="DomainErrorModel.None"/> on success.</summary>
    public DomainErrorModel DomainError { get; }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>The successful result.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, DomainErrorModel.None);
    }

    /// <summary>
    ///     Creates a failed result.
    /// </summary>
    /// <param name="error">The domain error that explains the failure.</param>
    /// <param name="errorEnum">The error code of the bounded context.</param>
    /// <returns>The failed result.</returns>
    public static Result<T> Failure(DomainErrorModel error, Enum errorEnum)
    {
        return new Result<T>(false, default, errorEnum, error);
    }

    /// <summary>
    ///     Converts the outcome to a single value by applying the function that matches it.
    /// </summary>
    /// <typeparam name="TResult">The type of the produced value.</typeparam>
    /// <param name="onSuccess">The function applied to the value of a success.</param>
    /// <param name="onFailure">The function applied to the domain error of a failure.</param>
    /// <returns>The value produced by the matching function.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainErrorModel, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(DomainError);
}
