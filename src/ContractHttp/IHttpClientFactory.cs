namespace ContractHttp
{
    using System.Net.Http;

    public interface IHttpClientFactory
    {
        HttpClient CreateClient();
    }
}