namespace ContractHttp
{
    /// <summary>
    /// An attribute used to state that a method call should build a delete request.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DeleteAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAttribute"/> class.
        /// </summary>
        public DeleteAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAttribute"/> class.
        /// </summary>
        /// <param name="template">A path template</param>
        public DeleteAttribute(string template)
        {
            this.Template = template;
        }
    }
}