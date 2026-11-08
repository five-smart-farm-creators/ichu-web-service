using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public class EdgeSynchronizationCommandServiceTests
{
    private static readonly Guid ReadingOne = Guid.Parse("88888888-8888-4888-8888-888888888888");
    private static readonly Guid ReadingTwo = Guid.Parse("99999999-9999-4999-8999-999999999999");
    private static readonly Guid UnknownDevice = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
    private static readonly Guid OtherRanch = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");

    private readonly FakeGateway _gateway = new();
    private readonly FakeDeviceAttributions _devices = new();
    private readonly FakeCattle _cattle = new();
    private readonly FakeReadings _readings = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly OperationsMonitoringOptions _options = new();
    private readonly EdgeSynchronizationCommandService _service;

    public EdgeSynchronizationCommandServiceTests()
    {
        _devices.Devices[TelemetryFixtures.DeviceId] =
            new DeviceAttribution(TelemetryFixtures.DeviceId, TelemetryFixtures.RanchId, TelemetryFixtures.CattleId);
        _cattle.Animals.Add(new CattleSnapshot(TelemetryFixtures.CattleId, TelemetryFixtures.RanchId,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), null));

        var readingService = new TelemetryReadingCommandService(_devices, _cattle, _readings, _unitOfWork, Options.Create(_options));
        _service = new EdgeSynchronizationCommandService(_gateway, readingService, new NoAlertCommands(),
            Options.Create(_options), new FakeTimeProvider(TelemetryFixtures.ReceivedAt));
    }

    [Fact]
    public async Task ANewCaptureIsAccepted()
    {
        var results = (await _service.Handle(Batch(Entry(ReadingOne)), CancellationToken.None)).Value!;

        Assert.Equal(SyncEntryStatus.Accepted, results.Single().Status);
        Assert.Single(_readings.Stored);
        Assert.Equal(1, _unitOfWork.Attempts);
    }

    [Fact]
    public async Task TheSameCaptureSentAgainIsADuplicate()
    {
        await _service.Handle(Batch(Entry(ReadingOne)), CancellationToken.None);

        var retry = (await _service.Handle(Batch(Entry(ReadingOne)), CancellationToken.None)).Value!;

        Assert.Equal(SyncEntryStatus.Duplicate, retry.Single().Status);
        Assert.Single(_readings.Stored);
    }

    [Fact]
    public async Task ADifferentCaptureUnderAStoredIdentifierIsAConflict()
    {
        await _service.Handle(Batch(Entry(ReadingOne, temperature: 37.2)), CancellationToken.None);

        var result = (await _service.Handle(Batch(Entry(ReadingOne, temperature: 38.9)), CancellationToken.None)).Value!.Single();

        Assert.Equal(SyncEntryStatus.Conflict, result.Status);
        Assert.Equal(OperationsMonitoringErrors.ReadingIdentifierConflict.Message, result.Reason);
        Assert.Single(_readings.Stored);
    }

    [Fact]
    public async Task AFailedSaveIsAConflictAndDoesNotFailTheBatch()
    {
        _unitOfWork.Succeeds = false;

        var result = (await _service.Handle(Batch(Entry(ReadingOne)), CancellationToken.None)).Value!.Single();

        Assert.Equal(SyncEntryStatus.Conflict, result.Status);
        Assert.Equal(OperationsMonitoringErrors.ReadingConcurrentlyStored.Message, result.Reason);
    }

    [Fact]
    public async Task AReadingFromAnUnregisteredDeviceIsRejected()
    {
        var result = (await _service.Handle(Batch(Entry(ReadingOne, deviceId: UnknownDevice)), CancellationToken.None)).Value!.Single();

        Assert.Equal(SyncEntryStatus.Rejected, result.Status);
        Assert.Equal(OperationsMonitoringErrors.DeviceNotRegistered.Message, result.Reason);
    }

    [Fact]
    public async Task AReadingFromAnotherRanchIsRejected()
    {
        _devices.Devices[TelemetryFixtures.DeviceId] = new DeviceAttribution(TelemetryFixtures.DeviceId, OtherRanch, null);

        var result = (await _service.Handle(Batch(Entry(ReadingOne)), CancellationToken.None)).Value!.Single();

        Assert.Equal(SyncEntryStatus.Rejected, result.Status);
        Assert.Equal(OperationsMonitoringErrors.DeviceGatewayAccessDenied.Message, result.Reason);
    }

    [Fact]
    public async Task ACaptureAheadOfTheServerClockIsRejected()
    {
        var result = (await _service.Handle(
            Batch(Entry(ReadingOne, recordedAt: TelemetryFixtures.ReceivedAt.AddMinutes(10))), CancellationToken.None)).Value!.Single();

        Assert.Equal(SyncEntryStatus.Rejected, result.Status);
        Assert.Equal(OperationsMonitoringErrors.CaptureTimeAhead.Message, result.Reason);
    }

    [Fact]
    public async Task AReadingWithAnInvalidMeasurementIsRejectedWithItsReason()
    {
        var result = (await _service.Handle(Batch(Entry(ReadingOne, activityIndex: -1)), CancellationToken.None)).Value!.Single();

        Assert.Equal(SyncEntryStatus.Rejected, result.Status);
        Assert.Equal(OperationsMonitoringErrors.ActivityIndexNegative.Message, result.Reason);
        Assert.Empty(_readings.Stored);
    }

    [Fact]
    public async Task ARejectedEntryDoesNotStopTheNextEntryOfTheBatch()
    {
        var results = (await _service.Handle(
            Batch(Entry(ReadingOne, deviceId: UnknownDevice), Entry(ReadingTwo)), CancellationToken.None)).Value!;

        Assert.Equal(new[] { SyncEntryStatus.Rejected, SyncEntryStatus.Accepted }, results.Select(result => result.Status).ToArray());
    }

    [Fact]
    public async Task ABatchFromAnUnknownGatewayIsRejected()
    {
        _gateway.Principal = null;

        var result = await _service.Handle(Batch(Entry(ReadingOne)), CancellationToken.None);

        Assert.Equal(OperationsMonitoringErrors.GatewayUnauthenticated.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task ABatchWithoutEntriesOrAboveTheLimitIsRejected()
    {
        var empty = await _service.Handle(Batch(), CancellationToken.None);
        _options.MaximumBatchEntries = 1;
        var oversized = await _service.Handle(Batch(Entry(ReadingOne), Entry(ReadingTwo)), CancellationToken.None);

        Assert.Equal(OperationsMonitoringErrors.InvalidEdgeBatchSize.Code, empty.DomainError.Code);
        Assert.Equal(OperationsMonitoringErrors.InvalidEdgeBatchSize.Code, oversized.DomainError.Code);
    }

    private static SynchronizeEdgeBatchCommand Batch(params EdgeReadingEntry[] readings) => new(readings, [], []);

    private static EdgeReadingEntry Entry(
        Guid readingId,
        Guid? deviceId = null,
        double? temperature = 37.2,
        double? activityIndex = null,
        DateTimeOffset? recordedAt = null) =>
        new(readingId, deviceId ?? TelemetryFixtures.DeviceId, recordedAt ?? TelemetryFixtures.CapturedAt,
            new CapturedMetric(temperature, null), activityIndex is null ? null : new CapturedMetric(activityIndex, null), null, null, null);

    private sealed class FakeGateway : IEdgeGatewayAuthenticator
    {
        public EdgeGatewayPrincipal? Principal { get; set; } = new(TelemetryFixtures.GatewayId, TelemetryFixtures.RanchId);

        public Task<EdgeGatewayPrincipal?> AuthenticateAsync(CancellationToken cancellationToken) => Task.FromResult(Principal);
    }

    private sealed class FakeDeviceAttributions : IDeviceAttributionLookup
    {
        public Dictionary<Guid, DeviceAttribution> Devices { get; } = [];

        public Task<DeviceAttribution?> FindAsync(Guid deviceId, DateTimeOffset capturedAt, CancellationToken cancellationToken) =>
            Task.FromResult(Devices.GetValueOrDefault(deviceId));
    }

    private sealed class FakeCattle : ICattleLookup
    {
        public List<CattleSnapshot> Animals { get; } = [];

        public Task<CattleSnapshot?> FindAsync(Guid cattleId, CancellationToken cancellationToken) =>
            Task.FromResult(Animals.SingleOrDefault(animal => animal.CattleId == cattleId));

        public Task<bool> LotBelongsToRanchAsync(Guid lotId, Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Edge batches of readings never check lots.");
    }

    private sealed class FakeReadings : ITelemetryReadingRepository
    {
        public List<TelemetryReading> Stored { get; } = [];

        public Task AddAsync(TelemetryReading reading, CancellationToken cancellationToken = default)
        {
            Stored.Add(reading);
            return Task.CompletedTask;
        }

        public Task<TelemetryReading?> FindByDeviceAndReadingIdAsync(Guid deviceId, Guid readingId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Stored.SingleOrDefault(reading => reading.DeviceId == deviceId && reading.ReadingId == readingId));

        public Task<IReadOnlyList<TelemetryReading>> FindByDeviceAndReadingIdsAsync(
            Guid deviceId,
            IReadOnlyCollection<Guid> readingIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TelemetryReading>>(
                Stored.Where(reading => reading.DeviceId == deviceId && readingIds.Contains(reading.ReadingId)).ToList());

        public Task<IReadOnlyList<TelemetryReading>> FindByCattleAsync(Guid cattleId, TimePeriod period, int skip, int take, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<int> CountByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<DateTimeOffset?> FindOldestRecordedAtByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TelemetryReading?> FindLatestByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TelemetryReading?> FindLatestValidByCattleAsync(Guid cattleId, TelemetryField field, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TelemetryReading?> FindLatestByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<TelemetryReading>> FindAttributedByRanchAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IOperationsMonitoringUnitOfWork
    {
        public bool Succeeds { get; set; } = true;

        public int Attempts { get; private set; }

        public Task<bool> TryCompleteAsync(CancellationToken cancellationToken = default)
        {
            Attempts++;
            return Task.FromResult(Succeeds);
        }
    }

    private sealed class NoAlertCommands : IMonitoringAlertCommandService
    {
        public Task<Result<MonitoringAlert>> Handle(AcknowledgeAlertCommand command, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Edge batches in these tests carry no alerts.");

        public Task<SyncEntryResult> Handle(SynchronizeAlertCommand command, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Edge batches in these tests carry no alerts.");

        public Task<SyncEntryResult> Handle(SynchronizeAlertChangeCommand command, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Edge batches in these tests carry no alerts.");
    }
}
