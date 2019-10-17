FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-buster AS build
LABEL maintainer="Kyle Dodson <kyledodson@gmail.com>"

# Restore NuGet dependencies.
WORKDIR /app
COPY Orleans.TelemetryConsumers.ECS.sln .
COPY src/Orleans.TelemetryConsumers.ECS/Orleans.TelemetryConsumers.ECS.csproj src/Orleans.TelemetryConsumers.ECS/
COPY test/Orleans.TelemetryConsumers.ECS.Tests/Orleans.TelemetryConsumers.ECS.Tests.csproj test/Orleans.TelemetryConsumers.ECS.Tests/
RUN dotnet restore Orleans.TelemetryConsumers.ECS.sln --runtime debian.10-x64

# Build project.
COPY . .
RUN dotnet build src/Orleans.TelemetryConsumers.ECS/Orleans.TelemetryConsumers.ECS.csproj --configuration Release --framework netstandard2.0 --no-restore --runtime debian.10-x64
RUN dotnet build test/Orleans.TelemetryConsumers.ECS.Tests/Orleans.TelemetryConsumers.ECS.Tests.csproj --configuration Release --framework netcoreapp3.0 --no-restore --runtime debian.10-x64
