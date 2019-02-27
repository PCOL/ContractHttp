namespace ContractHttp
{
    /// <summary>
    /// An attribute used to state that a method call should build a patch request.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   public class PatchAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatchAttribute"/> class.
        /// </summary>
        public PatchAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatchAttribute"/> class.
        /// </summary>
        /// <param name="template">A path template.</param>
        public PatchAttribute(string template)
        {
            this.Template = template;
        }
    }
}