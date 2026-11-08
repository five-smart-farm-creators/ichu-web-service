namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
