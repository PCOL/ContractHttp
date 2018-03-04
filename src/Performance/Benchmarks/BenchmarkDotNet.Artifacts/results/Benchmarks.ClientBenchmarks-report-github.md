``` ini

BenchmarkDotNet=v0.10.12, OS=galliumos 2.1
Intel Celeron CPU N2840 2.16GHz, 1 CPU, 2 logical cores and 2 physical cores
.NET Core SDK=2.0.0
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT DEBUG
  DefaultJob : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT


```
|               Method |       Mean |    Error |    StdDev |     Median |
|--------------------- |-----------:|---------:|----------:|-----------:|
|            SimpleGet |   892.7 us | 57.79 us | 163.00 us |   814.2 us |
|       SimpleGetAsync |   913.2 us | 17.72 us |  17.41 us |   911.7 us |
|      SimpleGetByName |   882.0 us | 17.39 us |  24.38 us |   874.0 us |
| SimpleGetByNameAsync | 1,057.5 us | 20.93 us |  39.31 us | 1,074.2 us |
