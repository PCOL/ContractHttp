``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-6820HQ CPU 2.70GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.103
  [Host]     : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT DEBUG
  DefaultJob : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT


```
|               Method |     Mean |    Error |    StdDev |
|--------------------- |---------:|---------:|----------:|
|            SimpleGet | 553.6 μs | 42.52 μs | 124.70 μs |
|       SimpleGetAsync |       NA |       NA |        NA |
|      SimpleGetByName | 172.0 μs |  3.33 μs |   4.67 μs |
| SimpleGetByNameAsync |       NA |       NA |        NA |

Benchmarks with issues:
  ClientBenchmarks.SimpleGetAsync: DefaultJob
  ClientBenchmarks.SimpleGetByNameAsync: DefaultJob
