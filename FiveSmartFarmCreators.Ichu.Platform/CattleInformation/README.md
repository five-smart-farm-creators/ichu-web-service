# Cattle Information

Maintains animal identity, productive-stage history, lifecycle and lot membership. This module implements the independent Cattle Information work for Giorgio Marzouk Awad Vargas. IoT assignments, telemetry, clinical records, campaigns and authorization remain owned by their respective contexts.

## Sources and status

- [Requirements, US-06 through US-09](https://github.com/five-smart-farm-creators/ichu-report/blob/develop/report/30-requirements-specification.md).
- [Design, section 4.2.2](https://github.com/five-smart-farm-creators/ichu-report/blob/develop/report/40-solution-software-design.md).
- [Domain vocabulary](https://github.com/five-smart-farm-creators/ichu-report/blob/develop/report/assets/diagram-sources/puml/class-cattle.puml) and [database design](https://github.com/five-smart-farm-creators/ichu-report/blob/develop/report/assets/diagram-sources/puml/db-cattle.puml).
- [Reference implementation](https://github.com/upc-pre-202620-1asi0572-8733/learning-center-platform/tree/main/Acme.Center.Platform/Profiles): `Application/CommandServices`, `Application/QueryServices`, `Application/Internal`, `Domain/Model`, `Domain/Repositories`, `Infrastructure/Persistence/EntityFrameworkCore`, `Interfaces/Rest/Resources`, `Interfaces/Rest/Transform` and `Interfaces/Acl`. Local contribution rules take precedence, including the `Async` suffix.

These are new module contracts. There were no existing Cattle Information contracts or shared persistence capabilities on the base branch. The host has not registered this module, authentication, its context, migrations, external adapters or an event relay. End-to-end operation and collar release are pending integration by the coordinator. No successful synchronization or external device state is simulated.

## Domain rules

- IDs and optimistic versions are nonempty UUIDs. Registration generates the animal ID and version. The ranch ID identifies the unit that owns the herd; it is not a request-supplied permission.
- An ear tag is trimmed, required and limited to 30 characters. Uniqueness is exact and case-sensitive within a ranch, including inactive animals. No extra punctuation or numeric format is imposed. The database unique index enforces concurrent registration safety.
- Registration requires breed, sex, birth date and current productive stage. It creates an active animal. Birth dates cannot be missing or future dates.
- Stage changes require an active animal and preserve the preceding interval. Dates cannot be future dates, precede birth or overlap recorded periods. There is no inferred sex/age transition matrix in the Report.
- Deactivation requires an active animal, reason and event date. It retains identity and history, closes the stage period and saves `CattleDeactivatedEvent`. Only `Death` represents mortality; analytics owns the calculation. No delete endpoint or automatic reactivation is provided.
- Lot membership requires an active animal from the same ranch. The relationship is many-to-many as shown by the Report's composite membership key. Duplicate assignment is harmless when its expected version is current. Removal does not delete the animal. Inactive membership is retained for reference; the active-lot facade excludes inactive animals.
- Lot names are required and limited to 80 characters; optional management criteria are limited to 120. Genealogy and paddock management are not required by the assigned stories and are not implemented.
- Every modification requires the latest `expectedVersion` of the modified animal or lot. Assignment also advances the cattle version so a concurrent deactivation cannot silently accept an assignment.

Implementation assumptions requiring contract review: the first known stage starts on the registration date, because registration supplies no historical start date. Date-only intervals use `[validFrom, validTo)`; stage changes cannot precede the current period. Same-day changes preserve the preceding stage with an empty date interval because the contract has no time-of-day precision. An exit may occur on the registration date. Birth-date corrections cannot move birth after an existing stage. Calendar validation currently uses the UTC date from `TimeProvider`; the coordinator should review ranch-local date requirements before integration. Ear-tag case normalization is not specified by the Report and is not inferred.

## HTTP contracts

All endpoints require authentication and server-side ranch permission. Read access is checked before looking up animal/lot data; writes require management access. Authorization is re-evaluated by `ICattleAccessService` on every application operation. The Iam adapter must validate current membership/advisory scope and expiry/revocation, reject unauthenticated callers and never trust roles or access flags from a request body.

The route prefix is `/api/v1/ranches/{ranchId}`.

| Method and path | Request | Success |
| --- | --- | --- |
| `POST /cattle` | `earTag`, `breed`, `sex`, `birthDate`, `lifeStage` | 201, animal resource and its profile location |
| `GET /cattle/{cattleId}` | None | 200, `{ cattle, device }` |
| `GET /cattle` | Optional `earTag`, `status`, `lotId`, `page=1`, `pageSize=50` | 200, array ordered by ear tag then ID; empty array when no matches |
| `PUT /cattle/{cattleId}` | `earTag`, `breed`, `sex`, `birthDate`, `expectedVersion` | 200, animal resource |
| `PATCH /cattle/{cattleId}/life-stage` | `lifeStage`, `validFrom`, `expectedVersion` | 200, animal resource with history |
| `POST /cattle/{cattleId}/deactivations` | `reason`, `exitDate`, `expectedVersion` | 200, inactive animal resource |
| `POST /lots` | `name`, optional `criterion` | 201, lot resource |
| `GET /lots` | None | 200, lot array |
| `PUT /lots/{lotId}/cattle/{cattleId}` | `expectedVersion` of the lot | 200, lot resource |
| `DELETE /lots/{lotId}/cattle/{cattleId}?expectedVersion={version}` | Version in query | 200, lot resource |

Search `earTag` is an exact trimmed match. Page sizes are 1 through 100; pages start at 1 and offsets must fit a 32-bit integer. `status=Active` limits inventory to active animals. Omit status to include inactive history. Lot resources retain their stored memberships; consumers must use the active facade when counting current animals.

Default ASP.NET JSON conventions remain unchanged: camelCase property names, UUID strings, `DateOnly` as `YYYY-MM-DD` and UTC timestamps as ISO 8601. Request body enums use the existing default numeric serialization; response resources render enum names as strings. Query enum binding accepts names. No measurement values or units are stored in this module.

| Enum | Numeric request values |
| --- | --- |
| Breed | `1=BrownSwiss`, `2=Fleckvieh`, `3=Criollo`, `4=Crossbred` |
| Sex | `1=Female`, `2=Male` |
| LifeStage | `1=Calf`, `2=Heifer`, `3=MilkingCow`, `4=FatteningBull` |
| CattleStatus | `1=Active`, `2=Inactive` |
| ExitReason | `1=Death`, `2=Sale`, `3=Theft`, `4=Culling` |

An animal resource contains `id`, `ranchId`, `earTag`, `breed`, `sex`, `birthDate`, `status`, `currentStage`, `exitReason`, `exitDate`, `version` and `stagePeriods`. Each stage period contains `id`, `lifeStage`, `validFrom`, `validTo`. Lot resources contain `id`, `ranchId`, `name`, `criterion`, `version` and memberships with `cattleId` and `assignedAt` (UTC).

The profile's `device` contains `isAvailable`, `collarId`, `lastReadingReceivedAt`. `isAvailable=true` with a null collar confirms no current assignment and requires a null reading timestamp. `isAvailable=false` means the integration could not supply device information; it must not be interpreted as no assigned collar. The integration adapter reads IoT Assets and Operations Monitoring through their contracts, without foreign table access or local copies. The last timestamp is the reception time, not an invented capture time or a claim of current telemetry.

Errors use Problem Details with a `code` extension:

| HTTP | Code | Meaning |
| --- | --- | --- |
| 400 | `invalid_input` | Invalid IDs, fields, enums, dates or search pagination |
| 401 | `unauthenticated` | No valid authenticated caller (also enforced by the host authentication handler) |
| 403 | `forbidden` | Caller lacks current access or management permission |
| 404 | `not_found` | Resource not found within the authorized ranch |
| 409 | `duplicate_ear_tag` | Ear tag already used; `conflictingCattleId` identifies the existing animal when available |
| 409 | `invalid_state` | Inactive modification, incompatible stage operation or lot membership |
| 409 | `conflict` | Stale expected version or database concurrency conflict |

ASP.NET request-binding/validation errors use its standard validation Problem Details. Infrastructure failures are not converted into successful responses.

Example registration:

```json
{
  "earTag": "ICHU-0001",
  "breed": 1,
  "sex": 1,
  "birthDate": "2024-05-20",
  "lifeStage": 3
}
```

## Application and cross-context contracts

`ICattleCommandService.HandleAsync(command, cancellationToken)` accepts `RegisterCattleCommand`, `UpdateCattleCommand`, `ChangeLifeStageCommand` and `DeactivateCattleCommand`, returning `Task<Cattle>` within this context. `ILotCommandService` handles create, assign and remove commands. `ICattleQueryService` handles `GetCattleByIdQuery` (profile) and `SearchCattleQuery` (paged animals); `ILotQueryService` handles `GetLotsByRanchQuery`.

Other contexts consume only `ICattleInformationContextFacade`, not entities or database tables:

```csharp
Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);
```

Snapshots contain ID, ranch ID, ear tag, status, productive stage, birth date, exit reason/date and version. A missing identity returns null; a missing/empty lot returns an empty list. Both methods enforce current read permission. Background processing needs an explicit authenticated service access adapter approved by Iam; there is no bypass. Planning can resolve active lot composition; IoT Assets can validate current identity/status; monitoring can preserve inactive identity for historical attribution. Temporal device assignment remains owned by IoT Assets.

`ICattleAccessService` exposes `EnsureReadAccessAsync(ranchId, token)` and `EnsureManageAccessAsync(ranchId, token)` against trusted server caller context. It must throw `CattleInformationException` with `unauthenticated` or `forbidden` on denial. No permissive implementation is registered.

`ICattleProfileIntegrationService.GetAsync(ranchId, cattleId, token)` returns `CattleDeviceDetails`. Its adapter is pending agreement with Alison and Flor. Returning explicit unavailable information is permitted for an unavailable integration; returning a fabricated no-collar result is not.

## Persistence and event delivery

Generic `CattleRepository<TContext>`, `LotRepository<TContext>` and `CattleUnitOfWork<TContext>` use the host's scoped EF `DbContext`. No competing context, shared repository or migration is introduced. Domain code has no ASP.NET Core or EF dependency.

`ApplyCattleInformationConfiguration()` maps schema `cattle` with tables `cattle`, `stage_periods`, `lots`, `lot_memberships` and `integration_events`. `cattle` is the collective plural. Columns are explicitly snake_case. Animal uniqueness is `(ranch_id, ear_tag)`; lot membership has a composite key and same-ranch composite foreign keys. Versions are concurrency tokens. No cross-context foreign key or foreign-table query is used.

Registration, stage changes and deactivation create `CattleRegisteredEvent`, `CattleLifeStageChangedEvent` and `CattleDeactivatedEvent`. Each has a stable `eventId`, UTC `occurredAt`, `ranchId`, `cattleId` and the relevant identity/stage/exit facts. Event payloads use camelCase names and the numeric enum values listed above. `CattleUnitOfWork` saves the aggregate/history and serialized event records atomically with EF's SaveChanges transaction. Failed saves do not clear domain events.

The host must provide the relay for rows whose `published_at` is null. It must deserialize only known event types, dispatch to idempotent consumers using `eventId`, retry failures and mark published only after all required consumers acknowledge delivery. In particular, IoT Assets must release the collar and preserve historical assignment, Planning must update future follow-ups, and analytics must distinguish death from other exits. `MarkPublished(utcTime)` is available on the record; there is no running relay or consumer in this branch. Delivery can repeat after acknowledgement loss, so consumers must deduplicate. This pending integration is required to complete US-09's collar-release criterion.

## Coordinator integration

Diego must connect the following outside this module:

1. Configure the host PostgreSQL context and call `modelBuilder.ApplyCattleInformationConfiguration()` from its model setup.
2. Implement the trusted Iam access adapter and IoT/monitoring profile adapter against agreed contracts.
3. Register the scoped host context, then call:

   ```csharp
   services.AddCattleInformation<HostDbContext, IamCattleAccessAdapter, CattleProfileIntegrationAdapter>();
   ```

   The type names above are placeholders for coordinator-owned implementations. All repositories and the unit of work must resolve the same scoped context. Use a scoped operation per request; dispose it after a failed save. Do not wrap these operations in an outer transaction without handling its rollback and event lifecycle.

4. Configure authentication, authentication middleware and authorization before mapping the controllers. The `[Authorize]` endpoints require a valid challenge scheme. Review permission mapping and denial auditing with Iam.
5. Generate/review/apply shared migrations; register the durable event relay and downstream consumers. No global configuration or credentials are included here.
6. Run real PostgreSQL and authenticated API integration checks, including concurrent duplicate registration, concurrent assignment/deactivation, history round-trips, denied/expired access and idempotent collar release.

After integration, run from the repository root:

```sh
dotnet restore ichu-web-service.sln
dotnet build ichu-web-service.sln --no-restore
dotnet test ichu-web-service.sln --no-build
dotnet run --project FiveSmartFarmCreators.Ichu.Platform --launch-profile http
```

The current base startup alone cannot serve these endpoints successfully because module dependency injection, database and authentication are deliberately left for coordinator integration. Build or domain checks do not establish completed integrations.

## Verification performed

The implementation was checked with an official .NET 10.0.403 SDK extracted into a temporary directory, without changing the global SDK or project dependencies.

| Check | Result |
| --- | --- |
| Solution restore and build with .NET 10 | Passed; the existing `Microsoft.AspNetCore` package reference produces warning NU1510 |
| `dotnet test ichu-web-service.sln --no-build --no-restore` | Completed successfully; the base solution contains no test project |
| Temporary domain/application harness using the actual sources | 26 domain checks and 16 application checks passed |
| Temporary EF harness using the actual module configurations and Npgsql provider | 11 model, SQL translation and change-tracking checks passed; no database connection |
| Temporary HTTP harness using the actual controllers and explicit access/persistence test doubles | 13 checks passed, including authentication denial, ranch access denial, registration location, duplicate identity, validation, stale versions, future stages and deactivation |
| Namespace/folder consistency and whitespace review | Passed |

The temporary harnesses were kept outside the repository; no test project, dependency or tooling was added. Domain checks cover history retention, same-day changes, invalid dates and enums, stale versions, cross-ranch changes, inactive guards, lot assignment/removal and concurrent-deactivation participation. Application checks cover denial before persistence/integration access, ranch-scoped identity, duplicate tags, profile composition and pagination. Real PostgreSQL round-trips, concurrent transactions, Iam revocation/expiry, external profile data and downstream event delivery were not verified and remain coordinator integration checks.
