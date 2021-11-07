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

```powershhell
.\build.ps1 -Run
```

After deployed, visit `https://localhost:5001`.

> Note: No need to build the application. No need to have the tooling (net5.0, global tools, etc.) installed. The bootstrapper will do this for you.

### Via docker

Modify application settings using environment file (`.env`) of modify the `environment` settings in `.\docker-compose.yml` file. The `.env` file will not be tracked by git.

This is an example of `.env` file:

```
DATABOX_TOKEN=111111111
GITHUB_ACCESSTOKEN=11111111
FACEBOOK_APPID=11111111
FACEBOOK_APPSECRET=111111111
```

After you have configured the environment variables, you can run the application:

```
docker-compose up -d
```

After deployed, visit: `http://localhost:5050`

> Note: Make sure you have docker installed and are using `linux` containers.
