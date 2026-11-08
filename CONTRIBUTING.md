# Contributing

Thank you for contributing to ICHU. This guide describes how to propose, implement, review, and release changes consistently.

## Development setup

- Use the .NET 10 SDK.
- Restore and build the solution with `dotnet restore ichu-web-service.sln` and `dotnet build ichu-web-service.sln`.
- Run the available automated tests with `dotnet test ichu-web-service.sln`. Add or update tests alongside behavior changes.

Keep changes focused, follow the existing C# conventions, and preserve nullable reference type annotations. Do not commit credentials, local configuration, build output, or unrelated formatting changes.

## Project structure

Keep the solution at the repository root, application code under `FiveSmartFarmCreators.Ichu.Platform/`, and architecture documentation under `docs/`.

As bounded contexts are implemented, use the agreed module names: `OperationsMonitoring`, `CattleInformation`, `IoTAssets`, `Planning`, `DashboardAnalytics`, `Iam`, and `SubscriptionPlans`. Keep shared technical capabilities in `Shared`; it is not a bounded context.

| Location within a context | Responsibility |
| --- | --- |
| `Domain/Model/` | Aggregates, entities, value objects, commands, queries, domain events, and domain errors. |
| `Domain/Repositories/` | Repository contracts. |
| `Domain/Services/` | Business rules that do not belong to a single entity or value object, when needed. |
| `Application/` | Use-case contracts, command/query services, event handlers, and orchestration. |
| `Infrastructure/` | EF Core mappings and repositories, external service adapters, and technical implementations. |
| `Interfaces/Rest/` | Controllers, request/response resources, and resource transformations. |
| `Interfaces/Acl/` | Explicit contracts exposed to other contexts, when needed. |

Match namespaces to folder paths, starting with `FiveSmartFarmCreators.Ichu.Platform`. Keep `Program.cs` focused on composition and startup, migrations under the application's `Migrations/`, and local launch settings under `Properties/`. Add folders only when required by approved work; do not create empty layers or unrelated directory structures.

## Code quality and reuse

- Write all source code, identifiers, comments, documentation, API descriptions, validation messages, and user-visible strings in English. Spanish text is not permitted.
- Keep classes and methods focused, readable, and organized by responsibility. Prefer explicit control flow and guard clauses over deep nesting or compressed expressions.
- Look for existing services, helpers, value objects, mappings, and contracts before adding equivalents. Reuse or extend them when their responsibility matches the requirement.
- Remove duplicated logic, unused code, unused imports, and obsolete comments within the changed scope. Reduce unnecessary code without sacrificing readability or domain boundaries.
- Keep reuse within the correct bounded context. Do not share domain entities, bypass another context's contract, or move business rules into `Shared` solely to avoid similar code.
- Use dependency injection for infrastructure dependencies. Keep controllers thin, application services focused on orchestration, and business invariants in the domain.
- Preserve nullable annotations, validate inputs at the appropriate boundary, and use asynchronous I/O with cancellation where supported. Avoid blocking asynchronous calls and logging credentials or sensitive data.
- Use four-space indentation, braces on separate lines, file-scoped namespaces, and one primary type per file. Place using directives before the namespace; organize members as fields, constructors, properties, and methods, grouping related behavior together.
- Follow the project's established patterns consistently. Add abstractions and dependencies only when they solve a concrete need; avoid speculative infrastructure and unrelated reformatting.

## Naming conventions

Use meaningful English domain terms and the following conventions consistently:

| Element | Convention | Example |
| --- | --- | --- |
| Types, namespaces, code folders, methods, properties, and constants | `PascalCase` | `Cattle`, `CattleInformation`, `RegisterCattle`, `RecordedAt`, `MaximumBatchSize` |
| Interfaces | `I` followed by `PascalCase` | `ICattleRepository` |
| Parameters and local variables | `camelCase` | `cattleId`, `recordedAt` |
| Private instance fields | `_camelCase` | `_cattleRepository` |
| C# files | Match the primary type name | `CattleRepository.cs` |
| Asynchronous methods | `Async` suffix, except framework-required signatures | `FindByIdAsync` |
| Commands and queries | Describe the operation and end in `Command` or `Query` | `RegisterCattleCommand`, `GetCattleByIdQuery` |
| Domain events | Describe a fact that has happened and end in `Event` | `CattleDeactivatedEvent` |
| API routes | Plural resource names in lowercase `kebab-case` | `/api/v1/device-assignments` |

Keep database table names plural and configure mappings explicitly. Keep C# naming separate from database and JSON serialization conventions; preserve existing public contracts unless the change is reviewed as a contract change.

These project rules adapt Microsoft's [C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) and [identifier naming conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names).

## Comments

- Write comments in English only. Explain business reasoning, constraints, or nonobvious decisions; avoid narrating code that is already clear.
- Use `// Comment text` for a single-line implementation comment.
- Use `/* ... */` for an implementation comment that spans multiple lines. Do not use consecutive `//` lines for a multiline explanation.
- Use `///` XML documentation only when documenting an API contract; it is separate from implementation comments.
- Keep comments concise and accurate. Do not add decorative separators, change-history notes, commented-out code, or explanations unrelated to the surrounding logic.

## Domain-Driven Design

Model the software around the language and rules of the farm-management domain, not around database tables or framework details.

- Use domain terminology consistently in names, APIs, tests, and documentation. Clarify unfamiliar or ambiguous terms with the team before introducing them.
- Identify bounded contexts and keep each context's model and vocabulary cohesive. Avoid sharing domain objects across contexts merely to reduce duplication; define explicit contracts when contexts need to communicate.
- Keep business rules and invariants with the domain concepts that own them. Represent meaningful identity and lifecycle with entities, equality by value with value objects, and transactional consistency boundaries with aggregates.
- Let aggregate roots control changes to their state. Validate invariants at domain boundaries instead of relying only on controllers, persistence constraints, or callers to behave correctly.
- Keep domain logic independent of ASP.NET Core, Entity Framework Core, and other infrastructure. Put use-case orchestration in the application layer, persistence and external integrations in infrastructure, and HTTP concerns in the API layer.
- Use domain events only for meaningful facts that have happened and when there is a concrete need to communicate them. Keep cross-context and external side effects behind explicit application or integration boundaries.
- Prefer a simple model that meets current requirements. Do not add patterns, layers, or abstractions without a domain or maintenance need.

When a change alters domain language, invariants, context boundaries, or architecture, update the relevant documentation and propose an Architecture Decision Record in `docs/adrs.md` when the decision is significant.

## Tests and quality

- Add or update unit tests for domain rules and edge cases. Add integration tests for persistence, API behavior, and interactions between components where practical.
- Test observable behavior and business invariants rather than implementation details.
- Ensure changes build and tests pass before opening a pull request. Include any relevant manual verification steps when behavior cannot be covered automatically.
- Update API or domain documentation when public behavior, terminology, or decisions change.

## Branching with Git Flow

Use Git Flow with `main` as the released, production-ready history and `develop` as the integration branch. Create short-lived branches from the indicated base:

| Branch | Branch from | Purpose and merge target |
| --- | --- | --- |
| `feature/<short-description>` | `develop` | New functionality; merge back into `develop` through a pull request. |
| `bugfix/<short-description>` | `develop` | Non-urgent fixes; merge back into `develop` through a pull request. |
| `release/vX.Y.Z` | `develop` | Stabilize a planned release; merge into `main` and back into `develop`. |
| `hotfix/vX.Y.Z` | `main` | Urgent production fixes; merge into `main` and back into `develop`. |

Use lowercase kebab-case descriptions, keep branches focused, and avoid committing directly to `main` or `develop`. Delete merged topic branches.

Never open a pull request from `develop` to `main`; releases always go through `release/vX.Y.Z`. Do not use other branch prefixes, such as `fix/`, `docs/`, `refactor/`, `chore/`, or `audit/`.

## Commit messages

Write commit messages in English using [Conventional Commits](https://www.conventionalcommits.org/):

```text
<type>(<scope>)[!]: <imperative summary>

[optional body]

[optional footer(s)]
```

Use a concise, imperative summary without a trailing period. Common types:

- `feat`: a user-facing capability or other new functionality.
- `fix`: a bug fix.
- `docs`: documentation-only changes.
- `test`: adding or correcting tests.
- `refactor`: restructuring without changing behavior.
- `perf`: a performance improvement.
- `build`, `ci`, `chore`, or `style`: build, automation, maintenance, or non-functional style changes.
- `revert`: revert a previous commit.

Commit messages must also follow these rules:

- Start the summary with a lowercase letter, and keep the header at most 72 characters.
- Use only these scopes: `iam`, `cattle-information`, `iot-assets`, `operations-monitoring`, `planning`, `dashboard-analytics`, `subscription-plans`, `shared`, `persistence`, `openapi`, `i18n`.
- Use `feat` only for new functionality, `docs` for documentation and diagram changes, and `chore` for changelog, version, or maintenance changes.
- Do not use the default message of the GitHub web editor, such as `Update file.md`.

Examples:

```text
feat(planning): schedule vaccination activities
fix(operations-monitoring): reject telemetry outside valid bounds
docs: clarify domain model contribution guidance
```

Invalid examples:

```text
(Docs): Add user personas
Update chapter02.md
feat: add controllers.
faet: add planning service
```

Mark breaking changes with `!` after the type/scope and explain them in the body or a `BREAKING CHANGE: <description>` footer. Reference issues in the body or footer when useful. Keep each commit coherent; do not use vague summaries such as `update` or `fix stuff`.

## Changelog

Record every logical code, configuration, documentation, or architecture change in [`CHANGELOG.md`](CHANGELOG.md) under `[Unreleased]` as part of the same pull request. Write one concise entry per logical change in English, describing its effect rather than listing commits or files.

Use the applicable Keep a Changelog category: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, or `Security`. Keep existing entries, add a category heading only when needed, and leave release sections for maintainers to update when a version is published.

## Semantic Versioning

Releases use `MAJOR.MINOR.PATCH` as defined by [Semantic Versioning 2.0.0](https://semver.org/). Version the released public contract, including externally observable API behavior:

- Increment **MAJOR** for incompatible changes to that contract.
- Increment **MINOR** for backwards-compatible functionality.
- Increment **PATCH** for backwards-compatible fixes.

Before the first stable `1.0.0` release, use `0.y.z` for development; the public contract is not yet considered stable, so compatibility guarantees are limited. Pre-release versions may use SemVer identifiers such as `1.2.0-rc.1`. Do not infer the version solely from a commit type: release maintainers review the aggregate changes, compatibility impact, and release notes before choosing a version. Tag releases on `main` as `vX.Y.Z`.

## Pull requests and review

Open pull requests against `develop`, except release and hotfix pull requests, which follow the Git Flow targets above. A pull request should:

- Explain the problem and the approach, including relevant domain context and any trade-offs.
- Link related issues or decisions and identify breaking changes.
- Include tests and documentation updates appropriate to the change.
- Pass the solution build and available tests, and receive review before merging.

Reviewers should focus on correctness, domain invariants, security, maintainability, tests, and compatibility. Resolve review feedback or explain why a proposed change is not needed before merging.

## Commit granularity

A commit is one complete logical change that can be described in a single sentence and leaves the project working.

| Too fine | Balanced | Too coarse |
| --- | --- | --- |
| One commit per folder or file (`feat: add commands models`, `feat: add events models`, ...) | `feat(iot-assets): add device aggregate with commands and events` | `feat: implement iot assets` (a whole bounded context) |
| Several consecutive typo commits | `docs(chapter-ii): add segment 1 interview analysis` | `docs: update report` |

- Code: a feature branch usually has 3 to 8 commits of roughly 3 to 10 files each, split by layer or use case (for example domain model, persistence, REST endpoints, tests).
- Documentation: one commit per complete artifact or section, not per paragraph and not per whole chapter.
- Group typo fixes with the section they belong to; a later round of fixes goes in a single `docs(<scope>): fix typos in <section>` commit.
- Before pushing a branch that only you use, squash local work-in-progress commits into logical commits. Never rewrite commits that are already published.
- The convention is enforced for commits created from 2026-11-07, when it was adopted; older history is kept unchanged.

## Enforcement

- After cloning, run once: `git config core.hooksPath .githooks`. The `commit-msg` hook rejects invalid messages locally.
- The `Git Flow checks` GitHub Actions workflow validates the branch name, the pull request target, and every commit message on pull requests to `main` and `develop`.
- Configure Git with your real name and the email linked to your GitHub account (`git config user.name` and `git config user.email`). `.mailmap` maps previously used alternative identities.
