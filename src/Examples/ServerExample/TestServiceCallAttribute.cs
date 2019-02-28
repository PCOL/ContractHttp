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
        }

        public override void OnExecuted(ServiceCallExecutedContext context)
        {
            Console.WriteLine("Executed");
        }
    }
}