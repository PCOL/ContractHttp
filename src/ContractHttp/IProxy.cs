namespace ContractHttp
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Defines the proxy interface.
    /// </summary>
    public interface IProxy
    {
        /// <summary>
        /// Gets or sets the method to invoke.
        /// </summary>
        Func<MethodInfo, object[], object> InvokeMethod { get; set; }

        /// <summary>
        /// Gets the proxy object.
        /// </summary>
        /// <returns>A proxy object.</returns>
        object GetProxyObject();
    }
}