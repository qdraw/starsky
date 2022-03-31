# Telemetry

Starsky collects completely anonymous telemetry data about general usage. 
Participation in this anonymous program is optional, 
and you may opt-out if you'd not like to share any information.

## Why Is Telemetry Collected?

Starsky has grown considerably since its release. 
Prior to telemetry collection, our improvement process has been very much a manual one.

For example, Starsky [dogfoods](https://en.wikipedia.org/wiki/Eating_your_own_dog_food) 
internal a large photo database.
Additionally, we actively engage with the community to gather feedback.
However, this approach only allows us to collect feedback from a subset of users. 
This subset may have different needs and use-cases than you.

Telemetry allows us to accurately gauge the applications feature usage, pain points, 
and customization across all users.
This data will let us better tailor the application to the masses, ensuring its continued growth, 
relevance, and user experience.
Furthermore, this will allow us to verify if improvements made to the application are improving 
the baseline of all applications.

## What Is Being Collected?

We track general usage information, such as Next.js plugins and build performance. 

Specifically, we track the following anonymously on startup:

    Version of Starsky
    General machine information 
        (e.g. number of CPUs, macOS/Windows/Linux,
         whether or not the command was run within Docker)

    Note: This list is regularly audited to ensure its accuracy.

You can view exactly what is being collected by setting the 
following environment variable: `app__EnablePackageTelemetryDebug=true`.
When this environment variable is set, data will not be sent to us. 
The data will only be printed out to the console stream, prefixed with [EnablePackageTelemetryDebug].

An example telemetry event looks like this:

```
AppSettingsAppVersionBuildDateTime= 03/25/2022 16:55:10
etc..
```


## What about Sensitive Data (e.g. Secrets)?

We do not collect any metrics which may contain sensitive data.
This includes, but is not limited to: environment variables, 
file paths, contents of files, logs, or serialized errors.

We take your privacy and our security very seriously. 
Starsky telemetry falls under the security disclosure policy.

## Will This Data Be Shared?

The data we collect is completely anonymous, not traceable to the source, 
and only meaningful in aggregate form. No data we collect is personally identifiable.

In the future, we plan to share relevant data with the community through public dashboards 
(or similar data representation formats).

## How Do I Opt-Out?

You may opt out-by running Starsky telemetry disable to set the following env variable:

`app__EnablePackageTelemetry=false`

You may check the status of telemetry collection at any time by running next telemetry status in the root of your project directory:

go to the application url: for example http://localhost:5000/api/env

You may re-enable telemetry if you'd like to re-join the program by running the following in the root of your project directory:

`app__EnablePackageTelemetry=true`

