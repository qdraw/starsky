```

BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.8246)
Intel Core Ultra 7 265HX 2.60GHz, 1 CPU, 20 logical and 20 physical cores
.NET SDK 8.0.420
  [Host]   : .NET 8.0.26 (8.0.26, 8.0.2626.16921), X64 RyuJIT x86-64-v3
  ShortRun : .NET 8.0.26 (8.0.26, 8.0.2626.16921), X64 RyuJIT x86-64-v3


```
| Method       | Job       | Toolchain              | IterationCount | LaunchCount | WarmupCount | Width | Height | Mean     | Error     | StdDev   | Median   | Gen0     | Gen1     | Gen2     | Allocated |
|------------- |---------- |----------------------- |--------------- |------------ |------------ |------ |------- |---------:|----------:|---------:|---------:|---------:|---------:|---------:|----------:|
| **TryLoad_8Bit** | **InProcess** | **InProcessEmitToolchain** | **Default**        | **Default**     | **Default**     | **4032**  | **3024**   | **25.37 ms** |  **0.511 ms** | **1.499 ms** | **25.52 ms** | **468.7500** | **468.7500** | **312.5000** |  **58.14 MB** |
| TryLoad_8Bit | ShortRun  | Default                | 3              | 1           | 3           | 4032  | 3024   | 26.03 ms | 18.011 ms | 0.987 ms | 26.03 ms | 406.2500 | 406.2500 | 250.0000 |  58.14 MB |
| **TryLoad_8Bit** | **InProcess** | **InProcessEmitToolchain** | **Default**        | **Default**     | **Default**     | **4032**  | **4024**   | **33.16 ms** |  **0.623 ms** | **0.583 ms** | **33.34 ms** | **687.5000** | **687.5000** | **375.0000** |  **77.37 MB** |
| TryLoad_8Bit | ShortRun  | Default                | 3              | 1           | 3           | 4032  | 4024   | 33.66 ms | 13.323 ms | 0.730 ms | 33.43 ms | 562.5000 | 562.5000 | 250.0000 |  77.37 MB |
| **TryLoad_8Bit** | **InProcess** | **InProcessEmitToolchain** | **Default**        | **Default**     | **Default**     | **6048**  | **3024**   | **41.15 ms** |  **1.505 ms** | **4.438 ms** | **39.36 ms** | **642.8571** | **642.8571** | **285.7143** |  **87.21 MB** |
| TryLoad_8Bit | ShortRun  | Default                | 3              | 1           | 3           | 6048  | 3024   | 40.39 ms | 32.537 ms | 1.783 ms | 39.77 ms | 692.3077 | 692.3077 | 307.6923 |  87.21 MB |
| **TryLoad_8Bit** | **InProcess** | **InProcessEmitToolchain** | **Default**        | **Default**     | **Default**     | **6048**  | **4024**   | **47.50 ms** |  **2.283 ms** | **6.730 ms** | **49.76 ms** | **333.3333** | **333.3333** | **333.3333** | **116.05 MB** |
| TryLoad_8Bit | ShortRun  | Default                | 3              | 1           | 3           | 6048  | 4024   | 46.32 ms | 13.602 ms | 0.746 ms | 46.54 ms | 636.3636 | 636.3636 | 181.8182 | 116.05 MB |
