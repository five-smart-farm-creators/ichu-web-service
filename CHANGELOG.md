# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Iam: added identity and ranch domain models, typed commands and queries, repository contracts and authorization rules.
- Iam: separated command and query service contracts, implementations and the application ACL facade.
- Iam: added scoped EF Core mappings and repositories, Firebase identity adapters and request identity resolution.
- Iam: added REST resources, assemblers, controllers and module registration following the reference DDD structure.
- Shared: added the shared kernel with the result and domain error model, base repository and unit of work, the PostgreSQL application context with audit timestamps, kebab-case routes and a global problem details exception handler.
- Iam: registered the module in the application host with Firebase settings read from configuration.
- Added the initial EF Core migration for the IAM schema, applied automatically on startup, and the local `dotnet-ef` tool.
- Added the Scalar API reference at `/scalar` in Development.
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

### Changed

- Iam: placed application result and port models alongside their command, query, and outbound service contracts.
- Iam: standardized XML documentation across domain, application, infrastructure, and REST contracts.
- Updated project dependencies to support the initial platform configuration.
- Required each logical code, configuration, documentation, or architecture change to have a concise changelog entry.

### Removed

- Removed the template `WeatherForecast` model and controller.
