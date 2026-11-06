using DomainErrorModel = FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors.Error;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

/// <summary>
///     Represents the outcome of an application operation that produces no value.
/// </summary>
public class Result : Result<object>
{
    private Result(bool isSuccess, string message, Enum? errorEnum, DomainErrorModel? error)
        : base(isSuccess, null, message, errorEnum, error)
    {
    }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    /// <returns>The successful result.</returns>
    public static Result Success()
    {
        return new Result(true, string.Empty, null, DomainErrorModel.None);
    }

    /// <summary>
    ///     Creates a failed result from a context-specific error enum and a message.
    /// </summary>
    /// <param name="error">The context-specific error enum.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>The failed result.</returns>
    public new static Result Failure(Enum error, string message)
    {
        return new Result(false, message, error, DomainErrorModel.Failure(error.ToString(), message));
    }

    /// <summary>
    ///     Creates a failed result from a domain error.
    /// </summary>
    /// <param name="error">The domain error.</param>
    /// <returns>The failed result.</returns>
    public new static Result Failure(DomainErrorModel error)
    {
        return new Result(false, error.Message, null, error);
    }

    /// <summary>
    ///     Creates a failed result from a domain error and its context-specific error enum.
    /// </summary>
    /// <param name="error">The domain error.</param>
    /// <param name="errorEnum">The context-specific error enum.</param>
    /// <returns>The failed result.</returns>
    public new static Result Failure(DomainErrorModel error, Enum errorEnum)
    {
        return new Result(false, error.Message, errorEnum, error);
    }
}
