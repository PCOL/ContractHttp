namespace ContractHttp
{
    /// <summary>
    /// An attribute used to state that a method call should build a get request.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class GetAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAttribute"/> class.
        /// </summary>
        public GetAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAttribute"/> class.
        /// </summary>
        /// <param name="template">The path template.</param>
        public GetAttribute(string template)
        {
            this.Template = template;
        }
    }
}