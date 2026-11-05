using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

public static class PagingGuard
{
    public static void Ensure(int page, int pageSize, int maximumPageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > maximumPageSize)
        {
            throw MonitoringException.InvalidInput($"The page must be positive and the page size between 1 and {maximumPageSize}.");
        }
    }
}
