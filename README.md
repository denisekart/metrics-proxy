# metrics-proxy

A solution to proxy analytics data from multiple source platforms into multiple target platforms. It's a fun exercise!

The application attempts to utilize the most of clean architecture design guidelines. Data sources and data sinks are considered as peripheral services and not a part of the core solution.

```json
<root>
├───docs // the design documentation
├───src // the source code
│   ├───DataSink.Multiple // data sinks
│   ├───DataSource.Multiple // data sources
│   ├───MetricsProxy.Application // the application logic
│   ├───MetricsProxy.Contracts // exposed contracts
│   ├───MetricsProxy.Tests // unit tests
│   └───MetricsProxy.Web // web runtime
```

## How to run

### Locally

Modify application settings using environment variables of `applicationSettings` file. An root application settings can be found at `src\MetricsProxy.Web\appSettings.json`. You can also create a file named `appSettings.Personal.json` in the same directory. That file will not be tracked by git.

This is an annotated example of `appSettings.Personal.json`:

```json
{
  "QueryService": {
    // The interval at which the querying and sending of KPIs will be triggered
    "IntervalInMilliseconds": 60000
  },
  "DataSink": {
    "Databox": {
      "Token": "111111"
    },
    "RandomlyFailing": {
      // The failure rate in percent(0-100) at which that sink will fail
      "FailureRatePercent": "0"
    } 
  },
  "DataSource": {
    "LinkedIn": {
      "ClientId": "111111",
      "ClientSecret": "111111"
    },
    "GitHub": {
      "AccessToken": "111111"
    },
    "Facebook": {
      "AppId": "111111",
      "AppSecret": "111111"
    }
  },
  // EfCore - sqlite database behind entity framework ORM
  // EfCore_Reset - the database will be reset after each start of the application
  // InMemory - an in memory database
  "DatabaseProvider": "EfCore_Reset" // InMemory | EfCore | EfCore_Reset
}
```

After you have configured your application, run it using `powershell`:

```pwsh
.\build.ps1 -Run
```

After deployed, visit `https://localhost:5001`.

> Note: No need to build the application. No need to have the tooling (net5.0, global tools, etc.) installed. The bootstrapper will do this for you.

### Via docker

Modify application settings using environment file (`.env`) of modify the `environment` settings in `.\docker-compose.yml` file. The `.env` file will not be tracked by git.

This is an example of `.env` file:

```cmd
DATABOX_TOKEN=111111111
GITHUB_ACCESSTOKEN=11111111
FACEBOOK_APPID=11111111
FACEBOOK_APPSECRET=111111111
```

After you have configured the environment variables, you can run the application:

```pwsh
docker-compose up -d
```

After deployed, visit: `http://localhost:5050`

> Note: Make sure you have docker installed and are using `linux` containers.


## How to test

You can run tests by executing the following command:

```pwsh
.\build.ps1 -Test
```

You can run test code coverage analysis by running:

```pwsh
.\build.ps1 -Cover
```

The resulting report will look like the following:

```txt
Summary
  Generated on: 7. 11. 2021 - 14:39:10
  Parser: MultiReportParser (29x CoberturaParser)
  Assemblies: 2
  Classes: 17
  Files: 12
  Line coverage: 95.5%
  Covered lines: 303
  Uncovered lines: 14
  Coverable lines: 317
  Total lines: 556

MetricsProxy.Application                                                 95.5%
  MetricsProxy.Application.Application.DefaultBackgroundServiceTracker  100.0%
  MetricsProxy.Application.Domain.DataSinkReportingService              100.0%
  MetricsProxy.Application.Domain.DataSourceQueryService                100.0%
  MetricsProxy.Application.Domain.MetricsManagementService               93.6%
  MetricsProxy.Application.Models.FailedStat                            100.0%
  MetricsProxy.Application.Models.KpiModel                              100.0%
  MetricsProxy.Application.Models.KpiStats                              100.0%
  MetricsProxy.Application.Models.KpiToReport                           100.0%
  MetricsProxy.Application.Models.ReportedKpi                           100.0%
  MetricsProxy.Application.Models.ReportTargetModel                     100.0%
  MetricsProxy.Application.Peripherals.Ef.Metric                        100.0%
  MetricsProxy.Application.Peripherals.Ef.MetricsContext                 71.4%
  MetricsProxy.Application.Peripherals.Ef.MetricTarget                   71.4%
  MetricsProxy.Application.Peripherals.EfCoreKpiRepository              100.0%
  MetricsProxy.Application.Peripherals.EfCoreKpiRepositoryExtensions     88.0%
  MetricsProxy.Application.Peripherals.InMemoryKpiRepository            100.0%

MetricsProxy.Contracts                                                  100.0%
  MetricsProxy.Contracts.Kpi                                            100.0%

```