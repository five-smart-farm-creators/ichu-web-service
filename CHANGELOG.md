# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added the initial ASP.NET Core web service foundation targeting .NET 10.
- Added PostgreSQL and Entity Framework Core dependencies for persistence.
- Added Humanizer and OpenAPI support.
- Added contributor guidance, architecture decision record guidance, a class diagram, and user-story documentation.
- Added proprietary licensing information.
- Added agent instructions and contribution guidance for the backend structure, DDD boundaries, coding standards, naming, comments, Git Flow, and Conventional Commits.
- Added README guidance for the technology stack, local setup, current service status, and planned bounded contexts.
- OperationsMonitoring: added Edge Gateway synchronization of readings, alerts and alert changes with individual results, capture-time attribution, six-hour alert deduplication, versioned alert attention, thresholds, geofences, revisioned Edge rules, field events, clinical interventions, offline mobile batches, telemetry queries and a read contract for other contexts.
- SubscriptionPlans: added the plan catalog, separate recurring and hardware estimates, pending subscription requests, verified payment confirmation, renewal, expiry, cancellation with payment history and a fail-closed entitlement contract for device limits and features.

### Changed

- Updated project dependencies to support the initial platform configuration.
- Required each logical code, configuration, documentation, or architecture change to have a concise changelog entry.

### Removed

- Removed the template `WeatherForecast` model and controller.
