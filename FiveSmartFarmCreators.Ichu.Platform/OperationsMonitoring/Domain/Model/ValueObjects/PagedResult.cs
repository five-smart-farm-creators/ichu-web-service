namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing one page of a query result.
/// </summary>
/// <typeparam name="T">The type of the listed items.</typeparam>
/// <param name="Items">The items of the page.</param>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
