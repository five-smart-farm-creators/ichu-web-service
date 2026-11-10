using System.ComponentModel.DataAnnotations;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record RegisterDeviceResource([Required, StringLength(50)] string Serial,
    [EnumDataType(typeof(DeviceType))] DeviceType Type,
    [StringLength(20)] string? FirmwareVersion);
