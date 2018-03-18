namespace ContractHttp
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHttpRequestSender
    {
        Task<HttpResponseMessage> SendAsync(
            IHttpRequestBuilder requestBuilder,
            HttpCompletionOption completionOption);
    }
}