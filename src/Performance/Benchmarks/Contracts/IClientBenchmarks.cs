namespace Benchmarks.Contracts
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Benchmarks.Contracts.Models;
    using ContractHttp;

    [HttpClientContract(Route = "api")]
    public interface IClientBenchmarks
    {
        [Get("")]
        IEnumerable<SimpleModel> Get();

        [Get("")]
        Task<IEnumerable<SimpleModel>> GetAsync();

        [Get("{name}")]
        SimpleModel GetByName(string name);

        [Get("{name}")]
        Task<SimpleModel> GetByNameAsync(string name);
    }
}