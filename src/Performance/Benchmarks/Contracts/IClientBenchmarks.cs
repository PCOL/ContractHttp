namespace Benchmarks.Contracts
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Benchmarks.Contracts.Models;
    using ContractHttp;
    using Microsoft.AspNetCore.Mvc;

    [HttpClientContract(Route = "api")]
    public interface IClientBenchmarks
    {
        [HttpGet("")]
        IEnumerable<SimpleModel> Get();

        [HttpGet("")]
        Task<IEnumerable<SimpleModel>> GetAsync();

        [HttpGet("{name}")]
        SimpleModel GetByName(string name);

        [HttpGet("{name}")]
        Task<SimpleModel> GetByNameAsync(string name);
    }
}