namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;

// Move to Shared/Application/Patterns when the shared kernel is available.
/// <summary>
///     Represents the outcome of an operation: either a success carrying a <typeparamref name="TValue"/> or a
///     failure carrying a <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TValue">The type of the value of a successful outcome.</typeparam>
/// <typeparam name="TError">The type of the error of a failed outcome.</typeparam>
public abstract record Result<TValue, TError>
{
    /// <summary>
    ///     A successful outcome.
    /// </summary>
    /// <param name="Value">The value produced by the operation.</param>
    public sealed record Success(TValue Value) : Result<TValue, TError>;

    /// <summary>
    ///     A failed outcome.
    /// </summary>
    /// <param name="Error">The error that explains the failure.</param>
    public sealed record Failure(TError Error) : Result<TValue, TError>;

    /// <summary>Gets a value indicating whether the outcome is a success.</summary>
    public bool IsSuccess => this is Success;

    /// <summary>Gets a value indicating whether the outcome is a failure.</summary>
    public bool IsFailure => this is Failure;

    /// <summary>
    ///     Transforms the value of a success and keeps the error of a failure.
    /// </summary>
    /// <typeparam name="TNext">The type of the transformed value.</typeparam>
    /// <param name="onSuccess">The function applied to the value of a success.</param>
    /// <returns>A result with the transformed value or the original error.</returns>
    public Result<TNext, TError> Map<TNext>(Func<TValue, TNext> onSuccess) =>
        this switch
        {
            Success success => new Result<TNext, TError>.Success(onSuccess(success.Value)),
            Failure failure => new Result<TNext, TError>.Failure(failure.Error),
            _ => throw new InvalidOperationException("Unknown result type.")
        };

    /// <summary>
    ///     Converts the outcome to a single value by applying the function that matches it.
    /// </summary>
    /// <typeparam name="TResult">The type of the produced value.</typeparam>
    /// <param name="onSuccess">The function applied to the value of a success.</param>
    /// <param name="onFailure">The function applied to the error of a failure.</param>
    /// <returns>The value produced by the matching function.</returns>
    public TResult Fold<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onFailure) =>
        this switch
        {
            Success success => onSuccess(success.Value),
            Failure failure => onFailure(failure.Error),
            _ => throw new InvalidOperationException("Unknown result type.")
        };

    /// <summary>
    ///     Runs the action that matches the outcome.
    /// </summary>
    /// <param name="onSuccess">The action run with the value of a success.</param>
    /// <param name="onFailure">The action run with the error of a failure.</param>
    public void Match(Action<TValue> onSuccess, Action<TError> onFailure)
    {
        if (this is Success success)
        {
            onSuccess(success.Value);
        }
        else if (this is Failure failure)
        {
            onFailure(failure.Error);
        }
    }
}
