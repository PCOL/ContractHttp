``` ini

BenchmarkDotNet=v0.10.12, OS=galliumos 2.1
Intel Celeron CPU N2840 2.16GHz, 1 CPU, 2 logical cores and 2 physical cores
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (Framework 4.6.26919.02), 64bit RyuJIT DEBUG
  DefaultJob : .NET Core 2.1.5 (Framework 4.6.26919.02), 64bit RyuJIT


```
|               Method |       Mean |    Error |   StdDev |
|--------------------- |-----------:|---------:|---------:|
|            SimpleGet |   855.5 us | 16.68 us | 16.38 us |
|       SimpleGetAsync | 1,022.7 us | 19.60 us | 20.12 us |
|      SimpleGetByName | 1,058.7 us | 20.82 us | 25.56 us |
| SimpleGetByNameAsync | 1,151.5 us | 19.79 us | 17.54 us |
