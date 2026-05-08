# Raw DNG Parser Benchmarks

This benchmark project measures `DngSubsetReader.TryLoad` throughput and allocations for common RAW payload layouts.

## What it benchmarks

- 8-bit uncompressed CFA payload
- 14-bit packed little-endian CFA payload
- 16-bit uncompressed CFA payload
- Two image sizes (`4032x3024` and `6048x4024`)

## Run

```powershell
cd C:\DEV\starsky\starsky
dotnet run -c Release --project benchmarks\starsky.benchmarks.rawdng\starsky.benchmarks.rawdng.csproj
```

Optional short run while iterating:

```powershell
cd C:\DEV\starsky\starsky
dotnet run -c Release --project benchmarks\starsky.benchmarks.rawdng\starsky.benchmarks.rawdng.csproj -- --job short
```

## Cross-platform notes

The benchmark is managed .NET 8 code only, so it runs on:

- Windows x64 (Intel/AMD)
- macOS x64
- macOS arm64
- Linux x64
- Linux arm64
- Linux arm (armhf)

Run on each target runtime to compare architecture-specific JIT behavior and memory bandwidth differences.
