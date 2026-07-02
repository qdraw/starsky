# Windows

WinUI 3 and WebView2 rewrite scaffold for Starsky Desktop.

## Scope

This project implements the Windows desktop shell described in [the functional spec](../documentation/docs/developer-guide/technologies/electron/starskydesktop-functional-spec.md).

The scaffold includes:

- Local and remote runtime modes
- Managed Starsky backend process startup for local mode
- WebView2-based main browser windows
- Route persistence and multi-window restore
- Desktop settings window
- Splash, error, and update warning windows
- Update-policy persistence
- Remote URL validation
- Desktop file download and open workflow
- File watcher setup for the local workspace

## Build

Use the local .NET 8 SDK on Windows:

```powershell
dotnet restore windows\windows.csproj
dotnet build windows\windows.csproj
```

## Notes

- This is an unpackaged Windows App SDK application.
- It expects a local Starsky runtime at `starsky/win-x64/starsky.exe` during development.
- For published builds, place the bundled runtime under `runtime-starsky-win-x64` next to the app executable.