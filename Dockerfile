# Builds the API with the SDK image and runs the published output on the smaller ASP.NET Core runtime image.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restoring from the project file first keeps the package layer cached while the sources change.
COPY FiveSmartFarmCreators.Ichu.Platform/FiveSmartFarmCreators.Ichu.Platform.csproj FiveSmartFarmCreators.Ichu.Platform/
RUN dotnet restore FiveSmartFarmCreators.Ichu.Platform/FiveSmartFarmCreators.Ichu.Platform.csproj

COPY . .
RUN dotnet publish FiveSmartFarmCreators.Ichu.Platform/FiveSmartFarmCreators.Ichu.Platform.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Only the published output is copied, so the SDK, the sources and the tests stay out of the runtime image.
COPY --from=build /app .

# The aspnet image provides the unprivileged app user. The port matches PORT in render.yaml.
USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FiveSmartFarmCreators.Ichu.Platform.dll"]
