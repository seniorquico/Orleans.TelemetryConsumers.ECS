# Orleans Host Environment Statistics for AWS Elastic Container Service (ECS)

[![codecov test status](https://codecov.io/gh/seniorquico/Orleans.TelemetryConsumers.ECS/branch/master/graph/badge.svg)](https://codecov.io/gh/seniorquico/Orleans.TelemetryConsumers.ECS) [![NuGet package version](https://img.shields.io/nuget/v/Orleans.TelemetryConsumers.ECS.svg?style=flat)](http://www.nuget.org/packages/Orleans.TelemetryConsumers.ECS/) [![MIT license](https://img.shields.io/badge/license-MIT-yellow.svg)](https://github.com/seniorquico/Orleans.TelemetryConsumers.ECS/blob/master/LICENSE) [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/orleans?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Provides [Microsoft Orleans](https://dotnet.github.io/orleans/index.html) host environment statistics (an `IHostEnvironmentStatistics` implementation) for [AWS Elastic Container Service (ECS)](https://aws.amazon.com/ecs/). This enables load shedding when silos are overloaded and silo metrics (CPU and memory) in the [Orleans Dashboard](https://github.com/OrleansContrib/OrleansDashboard).

This implementation relies on the [ECS Task Metadata Endpoint V3](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-metadata-endpoint-v3.html). This endpoint is available to all tasks running on the following AWS platforms:

- EC2 launch type with container agent version 1.21.0 or later
- Fargate launch type on platform version 1.3.0 or later

This endpoint may also be made available to containers running locally (for development and testing purposes) using the [Amazon ECS Local Container Endpoints](https://github.com/awslabs/amazon-ecs-local-container-endpoints).

## Installation

This is published as a Nuget package and depends on Orleans 3.0 (or greater).

### PackageReference

```xml
<PackageReference Include="Orleans.TelemetryConsumers.ECS" Version="1.0.0" />
```

### Package Manager

```powershell
Install-Package Orleans.TelemetryConsumers.ECS -Version 1.0.0
```

### NuGet CLI

```shell
dotnet add package Orleans.TelemetryConsumers.ECS --version 1.0.0
```

### Paket CLI

```shell
paket add Orleans.TelemetryConsumers.ECS --version 1.0.0
```

## Usage

First, ensure your ECS tasks are running on a supported platform (see the introduction for EC2 and Fargate platform requirements). If running on an unsupported platform, the host environment statistics will be unavailable.

Second, register the ECS `IHostEnvironmentStatistics` implementation and its dependencies:

```c#
new SiloHostBuilder()
  .UseEcsTaskHostEnvironmentStatistics()
  .Build();
```
