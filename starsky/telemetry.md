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

We track general usage information 

Specifically, we track the following anonymously on startup:

```
[EnablePackageTelemetryDebug] UTCTime - 04/04/2022 20:24:02
[EnablePackageTelemetryDebug] AppVersion - 0.5.0
[EnablePackageTelemetryDebug] NetVersion - .NET 6.0.3
[EnablePackageTelemetryDebug] OSArchitecture - X64
[EnablePackageTelemetryDebug] ProcessArchitecture - X64
[EnablePackageTelemetryDebug] OSVersion - 12.3.0
[EnablePackageTelemetryDebug] OSDescriptionLong - Darwin 21.4.0 
[EnablePackageTelemetryDebug] OSPlatform - OSX
[EnablePackageTelemetryDebug] DockerContainer - False
[EnablePackageTelemetryDebug] CurrentCulture - ivl
[EnablePackageTelemetryDebug] AspNetCoreEnvironment - Development
[EnablePackageTelemetryDebug] AppSettingsAppVersionBuildDateTime - 04/04/2022 22:23:46
[EnablePackageTelemetryDebug] AppSettingsName - Starsky
[EnablePackageTelemetryDebug] AppSettingsDatabaseType - Sqlite
[EnablePackageTelemetryDebug] AppSettingsStructure - /yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext
[EnablePackageTelemetryDebug] AppSettingsCameraTimeZone - Europe/Berlin
[EnablePackageTelemetryDebug] AppSettingsExifToolImportXmpCreate - True
[EnablePackageTelemetryDebug] AppSettingsReadOnlyFolders - ["/0001"]
[EnablePackageTelemetryDebug] AppSettingsAddMemoryCache - True
[EnablePackageTelemetryDebug] AppSettingsAddSwagger - True
[EnablePackageTelemetryDebug] AppSettingsAddSwaggerExport - True
[EnablePackageTelemetryDebug] AppSettingsAddSwaggerExportExitAfter - False
[EnablePackageTelemetryDebug] AppSettingsMetaThumbnailOnImport - True
[EnablePackageTelemetryDebug] AppSettingsPublishProfiles - {"_default":[{"ContentType":"Html","SourceMaxWidth":100,"OverlayMaxWidth":100,"Path":"warning: The field is not empty but for security reasons it is not shown","Folder":"","Append":"","Template":"Index.cshtml","Prepend":"","MetaData":true,"Copy":true},{"ContentType":"Html","SourceMaxWidth":100,"OverlayMaxWidth":100,"Path":"warning: The field is not empty but for security reasons it is not shown","Folder":"","Append":"","Template":"Index.cshtml","Prepend":"warning: The field is not empty but for security reasons it is not shown","MetaData":true,"Copy":true},{"ContentType":"Html","SourceMaxWidth":100,"OverlayMaxWidth":100,"Path":"warning: The field is not empty but for security reasons it is not shown","Folder":"","Append":"","Template":"Autopost.cshtml","Prepend":"warning: The field is not empty but for security reasons it is not shown","MetaData":true,"Copy":true},{"ContentType":"Jpeg","SourceMaxWidth":1000,"OverlayMaxWidth":380,"Path":"warning: The field is not empty but for security reasons it is not shown","Folder":"1000/","Append":"_kl1k","Template":"","Prepend":"","MetaData":true,"Copy":true},{"ContentType":"Jpeg","SourceMaxWidth":500,"OverlayMaxWidth":200,"Path":"warning: The field is not empty but for security reasons it is not shown","Folder":"500/","Append":"_kl","Template":"","Prepend":"","MetaData":false,"Copy":true},{"ContentType":"MoveSourceFiles","SourceMaxWidth":100,"OverlayMaxWidth":100,"Path":"","Folder":"orgineel/","Append":"","Template":"","Prepend":"","MetaData":true,"Copy":false},{"ContentType":"PublishContent","SourceMaxWidth":100,"OverlayMaxWidth":100,"Path":"","Folder":"","Append":"","Template":"","Prepend":"","MetaData":true,"Copy":true},{"ContentType":"PublishManifest","SourceMaxWidth":100,"OverlayMaxWidth":100,"Path":"","Folder":"","Append":"","Template":"","Prepend":"","MetaData":true,"Copy":true},{"ContentType":"OnlyFirstJpeg","SourceMaxWidth":213,"OverlayMaxWidth":100,"Path":"","Folder":"","Append":"___og_image","Template":"","Prepend":"","MetaData":false,"Copy":true}],"no_logo_2000px":[{"ContentType":"Jpeg","SourceMaxWidth":2000,"OverlayMaxWidth":100,"Path":"","Folder":"","Append":"_kl2k","Template":"","Prepend":"","MetaData":true,"Copy":true}]}
[EnablePackageTelemetryDebug] AppSettingsIsAccountRegisterOpen - True
[EnablePackageTelemetryDebug] AppSettingsNoAccountLocalhost - True
[EnablePackageTelemetryDebug] AppSettingsAccountRegisterDefaultRole - User
[EnablePackageTelemetryDebug] AppSettingsApplicationInsightsLog - True
[EnablePackageTelemetryDebug] AppSettingsApplicationInsightsDatabaseTracking - True
[EnablePackageTelemetryDebug] AppSettingsMaxDegreesOfParallelism - 6
[EnablePackageTelemetryDebug] AppSettingsUseHttpsRedirection - False
[EnablePackageTelemetryDebug] AppSettingsUseRealtime - True
[EnablePackageTelemetryDebug] AppSettingsUseDiskWatcher - True
[EnablePackageTelemetryDebug] AppSettingsCheckForUpdates - True
[EnablePackageTelemetryDebug] AppSettingsEnablePackageTelemetryDebug - True
[EnablePackageTelemetryDebug] FileIndexItemTotalCount - 1
[EnablePackageTelemetryDebug] FileIndexItemDirectoryCount - 0
[EnablePackageTelemetryDebug] FileIndexItemCount - 1
```

>     Note: This list is regularly audited to ensure its accuracy.

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

You may check the status of telemetry collection at any time by running the app telemetry status in the root of your project directory:

go to the application url: for example http://localhost:4000/api/env

You may re-enable telemetry if you'd like to re-join the program by running the following in the root of your project directory:

`app__EnablePackageTelemetry=true`

