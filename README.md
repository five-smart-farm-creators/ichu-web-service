# ICHU Web Service

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![C# 14.0](https://img.shields.io/badge/C%23-14.0-239120?style=flat)](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
[![ASP.NET Core 10.0](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-10.0)
[![Entity Framework Core 10.0.14](https://img.shields.io/badge/EF%20Core-10.0.14-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![PostgreSQL planned](https://img.shields.io/badge/PostgreSQL-planned-4169E1?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

Backend for the ICHU livestock management platform by SmartFarm. It is designed to serve the web and mobile applications and receive synchronized data from the Edge Gateway.

The project currently contains the ASP.NET Core service foundation and OpenAPI configuration. Business modules, authentication, and database integration are planned work.


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

The current startup does not connect to a database. Database configuration will be needed when persistence is implemented. Keep credentials outside version-controlled settings.


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
