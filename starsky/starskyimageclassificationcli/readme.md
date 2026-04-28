# starskyimageclassificationcli

Background worker CLI that pumps and processes image-classification jobs using the `ImageClassification` queue lane.

## What it does

- Forces queue backend for `ImageClassification` to `Database`
- Enqueues newest items in batches (`DateTime` desc, fallback `LastEdited`)
- Runs queue consumer and executes Ollama-based classification

## Config notes

- Uses normal Starsky appsettings loading (`appsettings.json`, patch/local/env overrides)
- Useful keys:
  - `app.useImageClassificationOnStartup`
  - `app.ollamaModel`
  - `app.ollamaExecutablePath`
  - `app.imageClassificationBatchSize`
  - `app.queue.databasePollIntervalInMilliseconds`

## Run locally

```powershell
dotnet run --project "C:\DEV\starsky\starsky\starskyimageclassificationcli\starskyimageclassificationcli.csproj"
```

## Optional: explicit config file

```powershell
$env:app__appsettingspath="C:\DEV\starsky\starsky\starsky\appsettings.default.json"
dotnet run --project "C:\DEV\starsky\starsky\starskyimageclassificationcli\starskyimageclassificationcli.csproj"
```

