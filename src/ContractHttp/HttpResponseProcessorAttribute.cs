namespace ContractHttp
{
    using System;

    /// <summary>
    /// Specifies the <see cref="HttpResponseProcessor{T}"/> to be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property)]
    public class HttpResponseProcessorAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseProcessorAttribute"/> class.
        /// </summary>
        /// <param name="processorType">The processor type.</param>
        public HttpResponseProcessorAttribute(Type processorType)
        {
            if (processorType.IsSubclassOfGeneric(typeof(HttpResponseProcessor<>)) == false)
            {
                throw new ArgumentException("Argument must be a response processor", "processorType");
            }

            this.ResponseProcesorType = processorType;
        }

        /// <summary>
        /// Gets the response processor type.
        /// </summary>
        public Type ResponseProcesorType { get; }
    }
}