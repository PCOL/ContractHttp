namespace ContractHttpTests.Resources
{
    using System.Net.Http;

    public interface IServiceResult<T>
    {
        T Result { get; }

        HttpResponseMessage Response { get; }
    }
}