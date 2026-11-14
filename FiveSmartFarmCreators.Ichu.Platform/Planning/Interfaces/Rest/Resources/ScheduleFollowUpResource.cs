using System.Text.Json.Serialization;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record ScheduleFollowUpResource([property: JsonRequired] DateTimeOffset DueAt);
