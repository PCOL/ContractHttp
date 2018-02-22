namespace ContractHttp
{
    public interface IAuthorizationHeaderFactory
    {
        string GetAuthorizationHeaderScheme();

        string GetAuthorizationHeaderValue();
    }
}