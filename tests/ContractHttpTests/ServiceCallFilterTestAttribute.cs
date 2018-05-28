namespace ContractHttpTests
{
    using ContractHttp;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A test service call filter attribute.
    /// </summary>
    public class ServiceCallFilterTestAttribute
        : ServiceCallFilterAttribute
    {
        /// <summary>
        /// On execute handler.
        /// </summary>
        /// <param name="context">The service executed context.</param>
        public override void OnExecuted(ServiceCallExecutedContext context)
        {
            context.Response = context.Controller.StatusCode(StatusCodes.Status400BadRequest);
        }
    }
}