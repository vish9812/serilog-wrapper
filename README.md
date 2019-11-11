# Serilog Wrapper

**Nuget-Package** : https://www.nuget.org/packages/SerilogWrapper/

A simple wrapper for Serilog.

It offers 5 types of logging
* Usage(Audit) - Tracks web requests
* Error - Provides global error handler
* Info - Logs general messages
* Diagnostic - Logs conditional/debugging messages
* Performance - Tracks timings of web requests

## Update Appsettings.json

```json
"Serilog": {
    "SelfLog": true,
    "Model": {
      "Product": "My Product XYZ",
      "LogSession": true
    },
    "Modules": {
      "Usage": true,
      "Error": true,
      "Info": true,
      "Diagnostic": true,
      "Performance": true
    },
    "Sql": {
      "SchemaName": "dbo",
      "AutoCreateSqlTable": false,
      "RestrictedToMinimumLevel": 1, //Verbose-0, Debug-1, Information-2, Warning-3, Error-4, Fatal-5
      "BatchPostingLimit": 100,
      "Period": "0.00:00:10",
      "TableName": {
        "Usage": "UsageLogs",
        "Error": "ErrorLogs",
        "Info": "InfoLogs",
        "Diagnostic": "DiagnosticLogs",
        "Performance": "PerformanceLogs"
      }
    }
  }
```

## Usage

* Register Logging dependecies

In Startup.cs -> ConfigureServices Method
```C#
services.RegisterLogger();
```

* Register Exception Logger

In Startup.cs -> Configure Method
```C#
//for MVC
app.UseMvcExceptionLogger();

//for API
app.UseAPIExceptionLogger();
```

* Register Usage and Performance Global Filters

In Startup.cs -> ConfigureServices Method
```C#
services.AddMvc(options =>
{
    options.Filters.Add<TrackUsageFilter>();
    options.Filters.Add<TrackPerformanceFilter>();
})
```

* To use any of the loggers directly, just inject the **ILogger**.
