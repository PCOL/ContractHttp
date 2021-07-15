namespace Benchmarks
{
    using System.Linq;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Validators;

    public class AllowNonOptimized
        : ManualConfig
    {
        public AllowNonOptimized()
        {
            Add(JitOptimizationsValidator.DontFailOnError);

            Add(DefaultConfig.Instance.GetLoggers().ToArray());
            Add(DefaultConfig.Instance.GetExporters().ToArray());
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
        }
    }
}