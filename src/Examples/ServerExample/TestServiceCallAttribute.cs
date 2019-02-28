namespace ServerExample
{
    using System;
    using ContractHttp;

    public class TestServiceCallAttribute
        : ServiceCallFilterAttribute
    {
        public override void OnExecuting(ServiceCallExecutingContext context)
        {
            Console.WriteLine("Executing");
            //context.Response = context.Controller.StatusCode(404);
        }

        public override void OnExecuted(ServiceCallExecutedContext context)
        {
            Console.WriteLine("Executed");
            //context.Response = context.Controller.StatusCode(409);
        }
    }
}