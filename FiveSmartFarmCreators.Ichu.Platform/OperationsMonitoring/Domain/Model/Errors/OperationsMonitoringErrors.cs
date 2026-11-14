using System.Globalization;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;

/// <summary>
///     Business errors of the Operations and Monitoring context. Each code is also the resource key of its localized message, so the English text lives in one place.
/// </summary>
public static class OperationsMonitoringErrors
{
    // Declared first because the error fields below read their messages during static initialization.
    private static readonly ResourceManager Messages = new(typeof(OperationsMonitoringMessages));

    public static readonly Error Unauthenticated = Define("Unauthenticated", EErrorType.Unauthorized);
    public static readonly Error GatewayUnauthenticated = Define("GatewayUnauthenticated", EErrorType.Unauthorized);
    public static readonly Error Forbidden = Define("Forbidden", EErrorType.Forbidden);
    public static readonly Error GatewayMismatch = Define("GatewayMismatch", EErrorType.Forbidden);
    public static readonly Error AlertGatewayAccessDenied = Define("AlertGatewayAccessDenied", EErrorType.Forbidden);
    public static readonly Error AlertChangeGatewayAccessDenied = Define("AlertChangeGatewayAccessDenied", EErrorType.Forbidden);
    public static readonly Error DeviceGatewayAccessDenied = Define("DeviceGatewayAccessDenied", EErrorType.Forbidden);

    public static readonly Error AlertNotFound = Define("AlertNotFound", EErrorType.NotFound);
    public static readonly Error AnimalNotFound = Define("AnimalNotFound", EErrorType.NotFound);
    public static readonly Error LotNotFound = Define("LotNotFound", EErrorType.NotFound);
    public static readonly Error GeofenceNotFound = Define("GeofenceNotFound", EErrorType.NotFound);
    public static readonly Error GeofenceNotInRanch = Define("GeofenceNotInRanch", EErrorType.NotFound);
    public static readonly Error AlertNotSynchronized = Define("AlertNotSynchronized", EErrorType.NotFound);
    public static readonly Error DeviceNotRegistered = Define("DeviceNotRegistered", EErrorType.NotFound);
    public static readonly Error WaterControllerNotFound = Define("WaterControllerNotFound", EErrorType.NotFound);
    public static readonly Error WaterTroughNotFound = Define("WaterTroughNotFound", EErrorType.NotFound);

    public static readonly Error GeofenceAlreadyDefined = Define("GeofenceAlreadyDefined", EErrorType.Conflict);
    public static readonly Error AlertConflict = Define("AlertConflict", EErrorType.Conflict);
    public static readonly Error OperationConflict = Define("OperationConflict", EErrorType.Conflict);
    public static readonly Error AnimalNotActive = Define("AnimalNotActive", EErrorType.Conflict);
    public static readonly Error ConcurrentModification = Define("ConcurrentModification", EErrorType.Conflict);
    public static readonly Error AlertIdentifierConflict = Define("AlertIdentifierConflict", EErrorType.Conflict);
    public static readonly Error AlertChangeConflict = Define("AlertChangeConflict", EErrorType.Conflict);
    public static readonly Error ReadingIdentifierConflict = Define("ReadingIdentifierConflict", EErrorType.Conflict);
    public static readonly Error ReadingConcurrentlyStored = Define("ReadingConcurrentlyStored", EErrorType.Conflict);
    public static readonly Error WaterReadingIdentifierConflict = Define("WaterReadingIdentifierConflict", EErrorType.Conflict);
    public static readonly Error WaterReadingConcurrentlyStored = Define("WaterReadingConcurrentlyStored", EErrorType.Conflict);
    public static readonly Error AlertConcurrentlyStored = Define("AlertConcurrentlyStored", EErrorType.Conflict);
    public static readonly Error AlertChangeConcurrentlyStored = Define("AlertChangeConcurrentlyStored", EErrorType.Conflict);
    public static readonly Error LivestockOperationConcurrentlyStored = Define("LivestockOperationConcurrentlyStored", EErrorType.Conflict);
    public static readonly Error ReadingAssignedAnimalNotActive = Define("ReadingAssignedAnimalNotActive", EErrorType.Conflict);
    public static readonly Error AlertAnimalNotActive = Define("AlertAnimalNotActive", EErrorType.Conflict);

    public static readonly Error EventTimeAhead = Define("EventTimeAhead", EErrorType.Validation);
    public static readonly Error InvalidPage = Define("InvalidPage", EErrorType.Validation);
    public static readonly Error InvalidRevision = Define("InvalidRevision", EErrorType.Validation);
    public static readonly Error InvalidEdgeBatchSize = Define("InvalidEdgeBatchSize", EErrorType.Validation);
    public static readonly Error InvalidMobileBatchSize = Define("InvalidMobileBatchSize", EErrorType.Validation);
    public static readonly Error RulesNotModified = Define("RulesNotModified", EErrorType.Failure);
    public static readonly Error PeriodRequired = Define("PeriodRequired", EErrorType.Validation);
    public static readonly Error PeriodStartNotBeforeEnd = Define("PeriodStartNotBeforeEnd", EErrorType.Validation);
    public static readonly Error AlertStatusUnknown = Define("AlertStatusUnknown", EErrorType.Validation);
    public static readonly Error AcknowledgedStatusOnly = Define("AcknowledgedStatusOnly", EErrorType.Validation);
    public static readonly Error ExpectedAlertVersionRequired = Define("ExpectedAlertVersionRequired", EErrorType.Validation);
    public static readonly Error ThresholdBoundsRequired = Define("ThresholdBoundsRequired", EErrorType.Validation);
    public static readonly Error MetricUnknown = Define("MetricUnknown", EErrorType.Validation);
    public static readonly Error SeverityUnknown = Define("SeverityUnknown", EErrorType.Validation);
    public static readonly Error AlertSeverityUnknown = Define("AlertSeverityUnknown", EErrorType.Validation);
    public static readonly Error AlertTypeUnknown = Define("AlertTypeUnknown", EErrorType.Validation);
    public static readonly Error ChangeKindUnknown = Define("ChangeKindUnknown", EErrorType.Validation);
    public static readonly Error ToleranceRequired = Define("ToleranceRequired", EErrorType.Validation);
    public static readonly Error VertexCoordinatesRequired = Define("VertexCoordinatesRequired", EErrorType.Validation);
    public static readonly Error ClinicalInterventionTypeOnly = Define("ClinicalInterventionTypeOnly", EErrorType.Validation);
    public static readonly Error FieldEventTypeOnly = Define("FieldEventTypeOnly", EErrorType.Validation);
    public static readonly Error RequiredFieldsMissing = Define("RequiredFieldsMissing", EErrorType.Validation);
    public static readonly Error OperationIdentifierRequired = Define("OperationIdentifierRequired", EErrorType.Validation);
    public static readonly Error LivestockIdentifiersRequired = Define("LivestockIdentifiersRequired", EErrorType.Validation);
    public static readonly Error ProductOrDoseNotClinical = Define("ProductOrDoseNotClinical", EErrorType.Validation);
    public static readonly Error ObservationTooLong = Define("ObservationTooLong", EErrorType.Validation);
    public static readonly Error ProductTooLong = Define("ProductTooLong", EErrorType.Validation);
    public static readonly Error DoseTooLong = Define("DoseTooLong", EErrorType.Validation);
    public static readonly Error SireReferenceTooLong = Define("SireReferenceTooLong", EErrorType.Validation);
    public static readonly Error PregnancyResultUnknown = Define("PregnancyResultUnknown", EErrorType.Validation);
    public static readonly Error PregnancyResultRequired = Define("PregnancyResultRequired", EErrorType.Validation);
    public static readonly Error ReproductiveDetailsNotAllowed = Define("ReproductiveDetailsNotAllowed", EErrorType.Validation);
    public static readonly Error ExpectedCalvingDateInvalid = Define("ExpectedCalvingDateInvalid", EErrorType.Validation);
    public static readonly Error ReproductiveEventRequiresFemale = Define("ReproductiveEventRequiresFemale", EErrorType.Validation);
    public static readonly Error CaptureTimeRequired = Define("CaptureTimeRequired", EErrorType.Validation);
    public static readonly Error CaptureTimeAhead = Define("CaptureTimeAhead", EErrorType.Validation);
    public static readonly Error EventTimeRequired = Define("EventTimeRequired", EErrorType.Validation);
    public static readonly Error ReadingIdentifiersRequired = Define("ReadingIdentifiersRequired", EErrorType.Validation);
    public static readonly Error AttributedAnimalRequired = Define("AttributedAnimalRequired", EErrorType.Validation);
    public static readonly Error ActivityIndexNegative = Define("ActivityIndexNegative", EErrorType.Validation);
    public static readonly Error RuminationMinutesNegative = Define("RuminationMinutesNegative", EErrorType.Validation);
    public static readonly Error BatteryLevelOutOfRange = Define("BatteryLevelOutOfRange", EErrorType.Validation);
    public static readonly Error HeartRateOutOfRange = Define("HeartRateOutOfRange", EErrorType.Validation);
    public static readonly Error MethaneIndexOutOfRange = Define("MethaneIndexOutOfRange", EErrorType.Validation);
    public static readonly Error MeasurementValidAndInvalid = Define("MeasurementValidAndInvalid", EErrorType.Validation);
    public static readonly Error MeasurementNotFinite = Define("MeasurementNotFinite", EErrorType.Validation);
    public static readonly Error InvalidMeasurementReason = Define("InvalidMeasurementReason", EErrorType.Validation);
    public static readonly Error PositionValidAndInvalid = Define("PositionValidAndInvalid", EErrorType.Validation);
    public static readonly Error PositionIncomplete = Define("PositionIncomplete", EErrorType.Validation);
    public static readonly Error PositionCoordinatesOutOfRange = Define("PositionCoordinatesOutOfRange", EErrorType.Validation);
    public static readonly Error PositionAccuracyInvalid = Define("PositionAccuracyInvalid", EErrorType.Validation);
    public static readonly Error CoordinateOutOfRange = Define("CoordinateOutOfRange", EErrorType.Validation);
    public static readonly Error ThresholdRangeInvalid = Define("ThresholdRangeInvalid", EErrorType.Validation);
    public static readonly Error ThresholdLifeStageLength = Define("ThresholdLifeStageLength", EErrorType.Validation);
    public static readonly Error ThresholdLevelsInconsistent = Define("ThresholdLevelsInconsistent", EErrorType.Validation);
    public static readonly Error RanchIdentifierRequired = Define("RanchIdentifierRequired", EErrorType.Validation);
    public static readonly Error GeofenceIdentifiersRequired = Define("GeofenceIdentifiersRequired", EErrorType.Validation);
    public static readonly Error GeofenceNameLength = Define("GeofenceNameLength", EErrorType.Validation);
    public static readonly Error GeofenceToleranceRange = Define("GeofenceToleranceRange", EErrorType.Validation);
    public static readonly Error GeofenceVertexCount = Define("GeofenceVertexCount", EErrorType.Validation);
    public static readonly Error GeofenceRepeatedVertices = Define("GeofenceRepeatedVertices", EErrorType.Validation);
    public static readonly Error GeofenceNotEnclosing = Define("GeofenceNotEnclosing", EErrorType.Validation);
    public static readonly Error GeofenceBoundaryCrossing = Define("GeofenceBoundaryCrossing", EErrorType.Validation);
    public static readonly Error AlertIdentifiersRequired = Define("AlertIdentifiersRequired", EErrorType.Validation);
    public static readonly Error AlertRevisionRequired = Define("AlertRevisionRequired", EErrorType.Validation);
    public static readonly Error AlertSupportingReadingRequired = Define("AlertSupportingReadingRequired", EErrorType.Validation);
    public static readonly Error AlertEvidenceNotAttributed = Define("AlertEvidenceNotAttributed", EErrorType.Validation);
    public static readonly Error AlertTypeNotThreshold = Define("AlertTypeNotThreshold", EErrorType.Validation);
    public static readonly Error GeofenceAlertRequiresGeofence = Define("GeofenceAlertRequiresGeofence", EErrorType.Validation);
    public static readonly Error GeofenceAlertDistanceInvalid = Define("GeofenceAlertDistanceInvalid", EErrorType.Validation);
    public static readonly Error GeofenceAlertDistanceRequired = Define("GeofenceAlertDistanceRequired", EErrorType.Validation);
    public static readonly Error ThresholdAlertRequiresThreshold = Define("ThresholdAlertRequiresThreshold", EErrorType.Validation);
    public static readonly Error AlertRevisionNotPublished = Define("AlertRevisionNotPublished", EErrorType.Validation);
    public static readonly Error ReadingNotSupportsAlert = Define("ReadingNotSupportsAlert", EErrorType.Validation);
    public static readonly Error EvidenceRequiresReading = Define("EvidenceRequiresReading", EErrorType.Validation);
    public static readonly Error ReturnRequiresGeofenceAlert = Define("ReturnRequiresGeofenceAlert", EErrorType.Validation);
    public static readonly Error ReturnPrecedesAlert = Define("ReturnPrecedesAlert", EErrorType.Validation);
    public static readonly Error AttentionAuthorRequired = Define("AttentionAuthorRequired", EErrorType.Validation);
    public static readonly Error AttentionFindingRequired = Define("AttentionFindingRequired", EErrorType.Validation);
    public static readonly Error AttentionFindingTooLong = Define("AttentionFindingTooLong", EErrorType.Validation);
    public static readonly Error AttentionActionTooLong = Define("AttentionActionTooLong", EErrorType.Validation);
    public static readonly Error AttentionPrecedesAlert = Define("AttentionPrecedesAlert", EErrorType.Validation);
    public static readonly Error SupportingReadingRequired = Define("SupportingReadingRequired", EErrorType.Validation);
    public static readonly Error SupportingReadingsRepeated = Define("SupportingReadingsRepeated", EErrorType.Validation);
    public static readonly Error SupportingReadingNotSynchronized = Define("SupportingReadingNotSynchronized", EErrorType.Validation);
    public static readonly Error EdgeAttentionAuthorRequired = Define("EdgeAttentionAuthorRequired", EErrorType.Validation);
    public static readonly Error EdgeAttentionVersionRequired = Define("EdgeAttentionVersionRequired", EErrorType.Validation);
    public static readonly Error WaterControllerGatewayAccessDenied = Define("WaterControllerGatewayAccessDenied", EErrorType.Forbidden);
    public static readonly Error WaterReadingInvalid = Define("WaterReadingInvalid", EErrorType.Validation);
    public static readonly Error WaterReadingPeriodPrecisionInvalid = Define("WaterReadingPeriodPrecisionInvalid", EErrorType.Validation);
    public static readonly Error WaterReadingEventsUnsupported = Define("WaterReadingEventsUnsupported", EErrorType.Validation);
    public static readonly Error WaterReadingPeriodOffsetRequired = Define("WaterReadingPeriodOffsetRequired", EErrorType.Validation);

    private static Error Define(string name, EErrorType type)
    {
        var code = $"OperationsMonitoring.{name}";
        return new Error(code, Messages.GetString(code, CultureInfo.InvariantCulture) ?? code, type);
    }
}
