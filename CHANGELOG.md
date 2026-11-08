# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Operations and Monitoring: Development seeds body temperature and activity thresholds for the seeded life stages and a geofence for the seeded lot, so that the edge raises alerts in demos. The values are placeholders, not validated clinical ranges.

## [0.3.0] - 2026-11-08

### Added

- Dashboard Analytics: indicators, lot trends, animal comparisons, epidemiological curves, portfolio priorities and executive reports are computed from the herd, readings, alerts and operations of the platform, with IAM ranch access and Subscription Plans export entitlements.
- Dashboard Analytics: registered the module in the application host, with English and Spanish messages for business failures.
- Dashboard Analytics: added the `DashboardAnalytics:ConditionIncubationDays` setting, whose values are placeholders until the team validates them.
- Cattle Information: added a herd read that lists every animal of a ranch, active and exited, with its current lot, for analytics.
- Operations and Monitoring: added read contracts that list the livestock operations of a ranch in a period and count its open alerts by severity, for analytics and portfolio priorities.

### Changed

- Dashboard Analytics: business failures are returned as results with stable error codes, and problem responses carry the localized message; exports without data answer 422 and inconsistent sources answer 503.
- Dashboard Analytics: a period may end after the current instant, so the current month can be analyzed.
- Shared: error types gained the unprocessable and unavailable kinds, so that a context can answer 422 and 503 through its results.

### Removed

- Dashboard Analytics: removed the analytics exception, its exception filter and the abstract controller base, since failures are no longer thrown.

## [0.2.0] - 2026-11-08

### Added

- Subscription Plans: added the plan catalog with annual price quotes by head count, subscription requests with idempotent retries, activation and cancellation rules, and an entitlements facade for other contexts.
- Subscription Plans: added the plans and subscriptions persistence mapping with the seeded catalog, a configurable terms version, and a Development-only Standard subscription for the Development ranch.
- Subscription Plans: added English and Spanish messages, with request-based culture selection and English as the default.
- Subscription Plans: the database allows only one pending or active subscription per ranch.
- Cattle Information: added the cattle schema migration for animals, stage periods, lots and lot memberships.
- Cattle Information: registered the module in the application host, with IAM ranch access checks and device details from IoT Assets and Operations and Monitoring.
- IoT Assets: added the iot schema migration for devices, device assignments and water troughs.
- IoT Assets: registered the module in the application host, with IAM access checks, animal lookups from Cattle Information and the collar limit from Subscription Plans.
- IoT Assets: a collar is released when its animal is deactivated, after the exit is saved.
- Shared: domain events are published after their changes are saved, and each handler runs in its own scope.
- Shared: the mediator pipeline behaviors and event handlers are registered for every bounded context.
- Cattle Information and IoT Assets: English and Spanish messages for business failures.
- IoT Assets: added the edge gateway registry with one-time keys, listing, lookup and revocation endpoints, and the iot.edge_gateways schema migration.
- Development: seeds the Development ranch's lot, animals, collars and Edge Gateway into the database when they are missing.
- Planning: added the planning schema migration in the shared database, with campaigns, planned activities, application records, follow-ups and withdrawal periods.
- Planning: registered the module in the application host, wired to IAM access scopes, Cattle Information animals and clinical interventions from Operations and Monitoring.
- Planning: added English and Spanish messages for business failures.
- Operations and Monitoring: the read contract resolves a clinical intervention by its operation identifier and returns nothing for field events.

### Changed

- Shared: unique-constraint violations and concurrent updates are answered with 409 Conflict instead of 500 Internal Server Error.
- Cattle Information: business failures are returned as results with stable error codes instead of exceptions, and duplicate ear tags report the animal that holds them.
- IoT Assets: business failures are returned as results with stable error codes, and problem responses carry the localized message.
- Operations and Monitoring: business failures are returned as results with stable error codes instead of exceptions, and problem responses carry the localized message and the error code.
- Operations and Monitoring: aggregates share the common aggregate root, the context identifiers are strongly typed, and read-only queries no longer track entities, without any schema change.
- Operations and Monitoring: messages are available in English and Spanish, and the code documents only the rules and reasons behind it.
- Operations and Monitoring: Edge Gateway keys have the form `{gatewayId}.{secret}` and are verified by IoT Assets, and animal, lot and collar lookups go through the Cattle Information and IoT Assets facades.
- Planning: business failures are returned as results with stable error codes, and problem responses carry the localized message.
- Planning: permissions follow the IAM access scopes. Reads need read access, campaigns and follow-ups need manage access, and applications and withdrawal periods need clinical-write access.
- Planning: an animal that belongs to another ranch answers 404 instead of 403, and a clinical intervention is linked only when it belongs to the same animal and ranch.

### Removed

- Cattle Information: removed the lifecycle outbox table and its mapping, since lifecycle events are now handled in-process after they are saved.
- Operations and Monitoring: removed the configuration-based simulation of animals, collars and Edge Gateways.
- Planning: removed the separate Planning database context, its schema initializer and the `ConnectionStrings:Planning` requirement, so the API starts again with the shared database.

## [0.1.0] - 2026-11-07

### Added

- Added Cattle Information registration, identification updates, ranch-scoped searches, productive-stage history, deactivation reasons, and same-ranch lot membership with optimistic concurrency.
- Added Cattle Information REST resources, explicit authorization and profile integration ports, cross-context identity queries, EF Core mappings, and atomic persistence of pending lifecycle events.
- Added Cattle Information contract documentation, Report traceability, and coordinator integration requirements for authentication, persistence, device information, and event delivery.
- Planning: Added campaign, activity, application, follow-up, calendar, and withdrawal-period contracts behind explicit persistence and bounded-context ports.
- Planning: Added PostgreSQL EF Core persistence, automatic schema initialization for a new Planning database, and Swagger UI for Development.
- Added IoT Assets inventory, collar assignment history, unlimited water-controller assignment, water-trough configuration, REST resources, EF Core mappings and integration contracts; Shared and host wiring remain pending.
- Iam: added identity and ranch domain models, typed commands and queries, repository contracts and authorization rules.
- Iam: separated command and query service contracts, implementations and the application ACL facade.
- Iam: added scoped EF Core mappings and repositories, Firebase identity adapters and request identity resolution.
- Iam: added REST resources, assemblers, controllers and module registration following the reference DDD structure.
- DashboardAnalytics: added authorized herd indicators, trends, animal comparisons, epidemiological curves, portfolio summaries and subscription-gated JSON and CSV exports.
- Shared: added the shared kernel with the result and domain error model, base repository and unit of work, the PostgreSQL application context with audit timestamps, kebab-case routes and a global problem details exception handler.
- Iam: registered the module in the application host with Firebase settings read from configuration.
- Added the initial EF Core migration for the IAM schema, applied automatically on startup, and the local `dotnet-ef` tool.
- Added Swagger UI at `/swagger` in Development, reading the built-in OpenAPI document.
- Added the bearer security scheme to the OpenAPI document so Swagger UI can send the Firebase identity token.
- Iam: exposed the current account identifier to other contexts through the context facade.
- Added the initial ASP.NET Core web service foundation targeting .NET 10.
- Added PostgreSQL and Entity Framework Core dependencies for persistence.
- Added Humanizer and OpenAPI support.
- Added contributor guidance, architecture decision record guidance, a class diagram, and user-story documentation.
- Added proprietary licensing information.
- Added agent instructions and contribution guidance for the backend structure, DDD boundaries, coding standards, naming, comments, Git Flow, and Conventional Commits.
- Added README guidance for the technology stack, local setup, current service status, and planned bounded contexts.
- Added the Operations and Monitoring bounded context with Edge Gateway telemetry synchronization, alert attention tracking, monitoring thresholds and geofences, livestock operations, and a read contract for other contexts.
- Operations and Monitoring: adopted the shared result, error, unit of work and repository model, authorized ranch operations through the IAM context facade, and added the EF Core migration for the monitoring schema.
- Operations and Monitoring: registered the module in the application host and simulated cattle, collars and Edge Gateways from configuration in Development.
- Operations and Monitoring: documented the required `X-Gateway-Key` header on the Edge Gateway operations in the OpenAPI document so Swagger UI can send the gateway key.
- Commit message hook and Git Flow pull request checks to enforce branch and Conventional Commits conventions.
- Git mailmap to unify contributor identities.

### Changed

- Placed Cattle Information query results and integration responses alongside their application contracts, removing the generic Application model folder.
- Documented Cattle Information domain types, lifecycle invariants, event semantics, and ranch-scoped repository contracts.
- Documented application and persistence contracts for access checks, versioned changes, host integration adapters, and atomic event delivery.
- Documented Cattle Information REST resources, endpoint behavior, and cross-context interfaces.
- DashboardAnalytics: separated typed query contracts, internal query services, outbound ports, domain value objects, and REST resources following the reference module structure.
- Iam: placed application result and port models alongside their command, query, and outbound service contracts.
- Iam: standardized XML documentation across domain, application, infrastructure, and REST contracts.
- Updated project dependencies to support the initial platform configuration.
- Required each logical code, configuration, documentation, or architecture change to have a concise changelog entry.
- Contribution guide aligned with the team's Git Flow, Conventional Commits and Semantic Versioning conventions.

### Removed

- Removed the module-level Cattle Information README.
- Removed the template `WeatherForecast` model and controller.
