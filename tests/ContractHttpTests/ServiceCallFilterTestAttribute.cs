namespace ContractHttpTests
{
    using ContractHttp;
    using Microsoft.AspNetCore.Http;

    public class ServiceCallFilterTestAttribute
        : ServiceCallFilterAttribute
    {
        public override void OnExecuted(ServiceCallExecutedContext context)
        {
            context.Response = context.Controller.StatusCode(StatusCodes.Status400BadRequest);
        }
    }
}