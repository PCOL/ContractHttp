namespace ContractHttp
{
    /// <summary>
    /// An attribute used to state that a method call should build a put request.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PutAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PutAttribute"/> class.
        /// </summary>
        public PutAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PutAttribute"/> class.
        /// </summary>
        /// <param name="template">A path template.</param>
        public PutAttribute(string template)
        {
            this.Template = template;
        }
    }
}