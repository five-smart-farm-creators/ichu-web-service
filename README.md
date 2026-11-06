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

The service needs a local PostgreSQL 17 database named `ichu` and applies pending EF Core migrations on startup. Keep credentials outside version-controlled settings and provide them as environment variables:

| Variable | Purpose |
| --- | --- |
| `ICHU_DB_PASSWORD` | Password of the `postgres` user, expanded into `ConnectionStrings:DefaultConnection`. |
| `Firebase__ProjectId` | Firebase project identifier used to verify identity tokens. |
| `Firebase__ApiKey` | Firebase Web API key used by the identity adapters. |

### Monitoring simulation data

Until the IoT Assets and Cattle Information modules exist, Development reads lots, cattle, collars and Edge Gateways from `OperationsMonitoring:Simulation` in `appsettings.Development.json`. These values are examples with fixed identifiers and are never used outside Development.

The `RanchId` values must match a ranch registered through IAM. Registering the development user as a `RanchManager` generates a new ranch identifier, so replace `11111111-1111-4111-8111-111111111111` with that identifier after registering. A simulated gateway authenticates by sending its `Key` in the `X-Gateway-Key` header, for example `POST /api/v1/telemetry/batch`; a request without a known key returns 401. In Swagger UI at `/swagger`, the telemetry batch and Edge Gateway monitoring rules operations show a required `X-Gateway-Key` input for that key.

Create a new migration after changing a mapping with `dotnet tool restore` and `dotnet ef migrations add <Name> --project FiveSmartFarmCreators.Ichu.Platform --output-dir Migrations`.


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
