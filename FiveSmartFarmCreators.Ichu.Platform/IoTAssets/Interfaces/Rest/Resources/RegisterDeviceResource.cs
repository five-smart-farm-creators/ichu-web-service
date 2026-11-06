using System.ComponentModel.DataAnnotations;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record RegisterDeviceResource([property: Required, StringLength(50)] string Serial,
    [property: EnumDataType(typeof(DeviceType))] DeviceType Type,
    [property: StringLength(20)] string? FirmwareVersion);
