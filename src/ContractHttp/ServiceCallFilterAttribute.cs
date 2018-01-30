namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class ServiceCallFilterAttribute
        : Attribute
    {

        public virtual void OnExecuting(ServiceCallExecutingContext context)
        {
        }

        public virtual void OnExecuted(ServiceCallExecutedContext context)
        {
        }
    }
}