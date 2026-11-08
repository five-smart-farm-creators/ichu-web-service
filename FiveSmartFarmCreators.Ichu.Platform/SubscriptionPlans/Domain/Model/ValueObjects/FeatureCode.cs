namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Capability a plan may grant. Other contexts gate their features on these codes.
/// </summary>
public enum FeatureCode { Monitoring, Alerts, Planning, Analytics, ReportExport }
