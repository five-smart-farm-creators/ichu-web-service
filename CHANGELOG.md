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

- Added the initial ASP.NET Core web service foundation targeting .NET 10.
- Added PostgreSQL and Entity Framework Core dependencies for persistence.
- Added Humanizer and OpenAPI support.
- Added contributor guidance, architecture decision record guidance, a class diagram, and user-story documentation.
- Added proprietary licensing information.
- Added agent instructions and contribution guidance for the backend structure, DDD boundaries, coding standards, naming, comments, Git Flow, and Conventional Commits.
- Added README guidance for the technology stack, local setup, current service status, and planned bounded contexts.

### Changed

- Iam: placed application result and port models alongside their command, query, and outbound service contracts.
- Iam: standardized XML documentation across domain, application, infrastructure, and REST contracts.
- Updated project dependencies to support the initial platform configuration.
- Required each logical code, configuration, documentation, or architecture change to have a concise changelog entry.

### Removed

- Removed the template `WeatherForecast` model and controller.
