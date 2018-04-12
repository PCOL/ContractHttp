namespace ContractHttpTests.Resources
{
    using System.Net.Http;

    public class ServiceResult<T>
        : IServiceResult<T>
    {
        public T Result { get; set; }

        public HttpResponseMessage Response { get; set; }
    }
}