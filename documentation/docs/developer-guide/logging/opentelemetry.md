# Open Telemetry logging

- Scroll down to [setup](#setup) for the settings

## Why OpenTelemetry?

OpenTelemetry has emerged as a powerful and 
standardized solution to address the challenges of observability in modern, 
distributed systems.

OpenTelemetry is an open-source project that provides a set of APIs, libraries, agents, 
and instrumentation to enable observability in cloud-native applications. 

Observability, in the context of software systems, 
refers to the ability to understand and measure how well a system is operating. 
It involves collecting and analyzing data related to key aspects such as traces, 
metrics, and logs. OpenTelemetry focuses on two primary pillars of observability: 
tracing and metrics.

### Distributed Tracing:
OpenTelemetry allows developers to trace requests as they traverse 
through various services in a distributed environment. 
This helps identify performance bottlenecks, understand dependencies between services, 
and diagnose issues across the entire application stack. 
By providing a standardized way to instrument code and capture trace data, 
OpenTelemetry simplifies the process of generating insights into the flow of requests in complex, 
microservices-based architectures.

### Metrics Collection:
Monitoring the health and performance of applications involves the collection of metrics. 
OpenTelemetry provides a consistent API for capturing metrics from different parts of an application. 
Metrics such as latency, error rates, and resource utilization 
can be collected and analyzed to gain a comprehensive view of application behavior. 
This information is invaluable for proactive issue detection, capacity planning, and overall system optimization.

OpenTelemetry supports multiple programming languages and integrates seamlessly with various observability backends, 
including popular solutions like Prometheus, Jaeger, and Grafana. 

Its adaptability and community-driven development make it a versatile choice for organizations seeking 
to implement observability in their applications, regardless of the technology stack they use.

In conclusion, OpenTelemetry plays a pivotal role in the modern software development landscape by providing 
a standardized and extensible framework for observability. 
Its ability to capture distributed traces and metrics empowers developers and operators to 
gain deep insights into the performance and behavior of their applications, 
facilitating efficient troubleshooting and continuous improvement.

## Setup
The following settings can be used:

### json configuration

use for example this file name: appsettings.machinename.json

### Order of appSettings patch files

1.  You can use `appsettings.json` inside the application folder to set base settings.
    The order of this files is used to get the values from the appsettings
    -    `/bin/Debug/net8.0/appsettings.patch.json`
    -    `/bin/Debug/net8.0/appsettings.default.json`
    -    `/bin/Debug/net8.0/appsettings.computername.patch.json`
    -    `/bin/Debug/net8.0/appsettings.json`
    -    `/bin/Debug/net8.0/appsettings.computername.json`

```json
{
    "app" : {
        "OpenTelemetry": {
            "TracesEndpoint": "http://test",
            "MetricsEndpoint": null,
            "LogsEndpoint": null
        }
    }
}
```


### Environment variables

```bash
"app__OpenTelemetry__TracesEndpoint": "https://otlp.eu01.nr-data.net:4318/v1/traces",
"app__OpenTelemetry__MetricsEndpoint": "https://otlp.eu01.nr-data.net:4318/v1/metrics",
"app__OpenTelemetry__LogsEndpoint": "https://otlp.eu01.nr-data.net:4318/v1/logs",
"app__OpenTelemetry__Header": "api-key=EXAMPLE_KEY",
"app__OpenTelemetry__ServiceName": "starsky-dev",
```

### Use a valid url
The properties assume that the Url is valid. It should start with http or https

```
Unhandled exception. System.UriFormatException: Invalid URI: The format of the URI could not be determined.
at System.Uri.CreateThis(String uri, Boolean dontEscape, UriKind uriKind, UriCreationOptions& creationOptions)
at System.Uri..ctor(String uriString)
at starsky.foundation.webtelemetry.Extensions.OpenTelemetryExtension.<>c__DisplayClass0_0.
<AddOpenTelemetryMonitoring>b__4(OtlpExporterOptions o) in ..Extensions/OpenTelemetryExtension.cs:line 48
```

