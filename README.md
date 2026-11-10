# ICHU Web Service

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![C# 14.0](https://img.shields.io/badge/C%23-14.0-239120?style=flat)](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
[![ASP.NET Core 10.0](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-10.0)
[![Entity Framework Core 10.0.14](https://img.shields.io/badge/EF%20Core-10.0.14-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![PostgreSQL planned](https://img.shields.io/badge/PostgreSQL-planned-4169E1?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

Backend for the ICHU livestock management platform by SmartFarm. It is designed to serve the web and mobile applications and receive synchronized data from the Edge Gateway.

The project contains the ASP.NET Core service foundation, the shared kernel, and the IAM module with Firebase authentication and PostgreSQL persistence. The remaining business modules are in progress.


## Run locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) and Git, then run:

```sh
git clone --branch develop https://github.com/five-smart-farm-creators/ichu-web-service.git
cd ichu-web-service
dotnet restore ichu-web-service.sln
dotnet build ichu-web-service.sln --no-restore
dotnet run --project FiveSmartFarmCreators.Ichu.Platform --launch-profile http
```

The HTTP profile starts the service at `http://localhost:5099` in Development. View the OpenAPI document at [http://localhost:5099/openapi/v1.json](http://localhost:5099/openapi/v1.json). Business endpoints will appear as controllers are implemented.

Resources that belong to a ranch are served under `/api/v1/ranches/{ranchId}/...`, and the animals of the domain language are routed as `cattle`.

The service needs a local PostgreSQL 17 database named `ichu` and applies pending EF Core migrations on startup. Keep credentials outside version-controlled settings and provide them as environment variables:

| Variable | Purpose |
| --- | --- |
| `ICHU_DB_PASSWORD` | Password of the `postgres` user, expanded into `ConnectionStrings:DefaultConnection`. |
| `Firebase__ProjectId` | Firebase project identifier used to verify identity tokens. |
| `Firebase__ApiKey` | Firebase Web API key used by the identity adapters. |

### Development data

Development seeds one ranch into the database when its records are missing: a lot, two cattle, three collars, one Edge Gateway and demonstration monitoring rules. The rules are body temperature 37.8–39.5 °C (Critical) and activity index 15–85 (Warning) for the `MilkingCow` and `Heifer` stages, and a geofence of about 1.1 km around latitude -16.5, longitude -68.1 with a 25 m tolerance (Warning). They are placeholders for demos, not validated clinical ranges. The ranch identifier is `11111111-1111-4111-8111-111111111111`, and the development gateway key is `c07d8c6e-6d9b-4cb7-9add-7b8c9daebfc6.development-gateway-key`. These fixed values match the Edge Gateway development seed and are never used outside Development.

IAM generates ranch identifiers when a ranch is registered, so the seeded ranch is visible to a user only through an IAM ranch with that identifier, which the demo account below owns. An Edge Gateway sends its key in the `X-Gateway-Key` header, for example `POST /api/v1/telemetry/batch`; a request without a known, unrevoked key returns 401. Gateways are registered at `POST /api/v1/ranches/{ranchId}/edge-gateways`, which returns the key once in the form `{gatewayId}.{secret}`. In Swagger UI at `/swagger`, the telemetry batch and Edge Gateway monitoring rules operations show a required `X-Gateway-Key` input for that key.

Create a new migration after changing a mapping with `dotnet tool restore` and `dotnet ef migrations add <Name> --project FiveSmartFarmCreators.Ichu.Platform --output-dir Migrations`.


### Demo account

A real Firebase user can see the seeded ranch in Swagger UI. Set it up once, in this order:

1. In the Firebase console, open Authentication, then Users, and add a user with an email and a password.
2. From a PowerShell 7 prompt, run `./scripts/firebase-dev-session.ps1 -Email <email>` and enter the password. The script reads `Firebase__ApiKey` from the user environment variables, as listed above. The first run sends a verification email and exits with code 1.
3. Open the verification link in that email, then run the script again. It saves the user identifier and the email as user environment variables, and copies an ID token to the clipboard. The token expires after one hour, so run the script again when it does.
4. Restart the API in Development with `dotnet run --project FiveSmartFarmCreators.Ichu.Platform --launch-profile http`, so that it seeds the ranch and the demo account.
5. Open `http://localhost:5099/swagger`, click **Authorize**, paste the token without the `Bearer` prefix, click **Authorize**, and close the dialog.
6. Call `GET api/v1/accounts/me/ranches`. The response lists ranch `11111111-1111-4111-8111-111111111111` with the scope `Manage`.

Without both `DevelopmentSeed__Manager__ExternalUid` and `DevelopmentSeed__Manager__Email`, the API skips the demo account and logs that it did.

### Analytics configuration

Epidemiological curves compare the observation window with the incubation period of each health condition, set in the `DashboardAnalytics:ConditionIncubationDays` section of `appsettings.json`. The shipped values (`temperature` and `activity`, 7 days each) are placeholders that the team must validate before the curves inform any decision. A condition without a configured value returns its curve with the reason that no incubation period is set.

## Tests

Run `dotnet test --configuration Release` from the repository root to run the unit tests and the HTTP integration tests. The integration tests need a PostgreSQL 17 server and a user environment variable named `ICHU_TEST_CONNECTION` that holds the connection string without a database, with the password written as a placeholder:

```text
Host=localhost;Port=5432;Username=postgres;Password=%ICHU_DB_PASSWORD%
```

Define `ICHU_DB_PASSWORD` as a user environment variable too, so that the password stays out of the repository and of the test output. Each run creates a database named `ichu_it_<8 hex characters>`, migrates it on startup, and drops it when the run ends. The integration tests host the API in the Testing environment and cover routing, the Firebase authentication handler with a fake identity provider, IAM access checks, controllers, EF Core persistence, and localized problem responses for every bounded context. They never reach Firebase. A test that documents a reported bug is skipped with a `Bug:` reason until the bug is fixed.

The BDD acceptance scenarios are Gherkin features under `tests/FiveSmartFarmCreators.Ichu.Platform.IntegrationTests/Features/<BoundedContext>/`, one per user or technical story, bound to reusable steps under `Steps/` with Reqnroll 3.3.4. Each feature carries its story identifier as a tag, such as `@US-15`, which becomes the `Category` trait, so `dotnet test --filter Category=US-15` runs only that story. The integration assembly runs one test at a time because the scenarios share a single API and database.


## Repository structure

```text
ichu-web-service/
├── ichu-web-service.sln
├── FiveSmartFarmCreators.Ichu.Platform/
│   ├── FiveSmartFarmCreators.Ichu.Platform.csproj
│   ├── FiveSmartFarmCreators.Ichu.Platform.http
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Properties/
│       └── launchSettings.json
├── docs/
│   ├── adrs.md
│   ├── class-diagram.puml
│   └── user-stories.md
├── CONTRIBUTING.md
├── AGENTS.md
├── CHANGELOG.md
└── LICENSE.md
```

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) for setup, DDD boundaries, coding standards, Git Flow, Conventional Commits, and pull request requirements. Write source code, comments, documentation, and commit messages in English.

## License

Copyright (c) 2026 SmartFarm. All rights reserved. This project is proprietary; see [LICENSE.md](LICENSE.md) for its terms.
