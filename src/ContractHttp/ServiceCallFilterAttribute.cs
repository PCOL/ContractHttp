namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute thata represents a service call filter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class ServiceCallFilterAttribute
        : Attribute
    {
        /// <summary>
        /// Called prior to a decorated method being called.
        /// </summary>
        /// <param name="context">The calling context.</param>
        public virtual void OnExecuting(ServiceCallExecutingContext context)
        {
        }

        /// <summary>
        /// Called after a decorated method has been called.
        /// </summary>
        /// <param name="context">The called context.</param>
        public virtual void OnExecuted(ServiceCallExecutedContext context)
        {
        }
    }
}