# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added Cattle Information registration, identification updates, ranch-scoped searches, productive-stage history, deactivation reasons, and same-ranch lot membership with optimistic concurrency.
- Added Cattle Information REST resources, explicit authorization and profile integration ports, cross-context identity queries, EF Core mappings, and atomic persistence of pending lifecycle events.
- Added Cattle Information contract documentation, Report traceability, and coordinator integration requirements for authentication, persistence, device information, and event delivery.
- Added the initial ASP.NET Core web service foundation targeting .NET 10.
- Added PostgreSQL and Entity Framework Core dependencies for persistence.
- Added Humanizer and OpenAPI support.
- Added contributor guidance, architecture decision record guidance, a class diagram, and user-story documentation.
- Added proprietary licensing information.
- Added agent instructions and contribution guidance for the backend structure, DDD boundaries, coding standards, naming, comments, Git Flow, and Conventional Commits.
- Added README guidance for the technology stack, local setup, current service status, and planned bounded contexts.

### Changed

- Documented Cattle Information domain types, lifecycle invariants, event semantics, and ranch-scoped repository contracts.
- Documented application and persistence contracts for access checks, versioned changes, host integration adapters, and atomic event delivery.
- Documented Cattle Information REST resources, endpoint behavior, and cross-context interfaces.
- Updated project dependencies to support the initial platform configuration.
- Required each logical code, configuration, documentation, or architecture change to have a concise changelog entry.

### Removed

- Removed the template `WeatherForecast` model and controller.
