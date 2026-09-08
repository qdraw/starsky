---
sidebar_position: 8
---

# WPF

Starsky for Windows is a native WPF application in the `windows/` folder.
It provides a desktop shell around the Starsky web interface using WebView2.

The client runs the bundled Starsky backend in local mode or connects to a configured remote server. Business logic remains in the .NET backend; the Windows client owns the native window lifecycle, menus, settings, and WebView2 integration.

## Test

Run the Microsoft Testing Platform test executable on Windows from the repository root:

```powershell
cd windows
dotnet run --project starsky.Tests/starsky.Tests.csproj
```