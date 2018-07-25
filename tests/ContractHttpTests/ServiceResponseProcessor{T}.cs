namespace ContractHttpTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class ServiceResponseProcessor<T>
        : HttpResponseProcessor<IServiceResult<T>>
    {
        /// <inheritdoc />
        public override async Task<IServiceResult<T>> ProcessResponseAsync(HttpResponseMessage response)
        {
            await Task.Yield();
            var result = new ServiceResult<T>();
            result.Response = response;

            if (response.IsSuccessStatusCode == true)
            {
                var content = await response.Content.ReadAsStringAsync();
                result.Result = JsonConvert.DeserializeObject<T>(content);
            }

            return result;
        }
    }
}