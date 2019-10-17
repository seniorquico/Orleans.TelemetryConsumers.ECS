@echo off
cd /D "%~dp0\.."
dotnet tool restore --verbosity minimal
dotnet restore --verbosity minimal
dotnet build --configuration Release --no-restore --verbosity minimal /p:TreatWarningsAsErrors=true /warnaserror
dotnet test --configuration Release --no-build --no-restore --verbosity minimal /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:Include="[Orleans.TelemetryConsumers.ECS]*"
dotnet reportgenerator -reports:.\test\Orleans.TelemetryConsumers.ECS.Tests\coverage.opencover.xml -reporttypes:HTML -targetdir:.\artifacts\Release\coverage-report
