# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Subscription Plans: added the plan catalog with annual price quotes by head count, subscription requests with idempotent retries, activation and cancellation rules, and an entitlements facade for other contexts.
- Subscription Plans: added the plans and subscriptions persistence mapping with the seeded catalog, a configurable terms version, and a Development-only Standard subscription for the simulated ranch.
- Subscription Plans: added English and Spanish messages, with request-based culture selection and English as the default.
- Subscription Plans: the database allows only one pending or active subscription per ranch.

### Changed

- Shared: unique-constraint violations are answered with 409 Conflict instead of 500 Internal Server Error.

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
