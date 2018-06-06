``` ini

BenchmarkDotNet=v0.10.12, OS=galliumos 2.1
Intel Celeron CPU N2840 2.16GHz, 1 CPU, 2 logical cores and 2 physical cores
.NET Core SDK=2.0.0
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT DEBUG
  DefaultJob : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT


```
|               Method |     Mean |     Error |    StdDev |   Median |
|--------------------- |---------:|----------:|----------:|---------:|
|            SimpleGet | 1.160 ms | 0.1264 ms | 0.3706 ms | 1.004 ms |
|       SimpleGetAsync | 1.575 ms | 0.1584 ms | 0.4645 ms | 1.439 ms |
|      SimpleGetByName | 1.047 ms | 0.0624 ms | 0.1739 ms | 1.013 ms |
| SimpleGetByNameAsync | 1.351 ms | 0.1020 ms | 0.2894 ms | 1.255 ms |
