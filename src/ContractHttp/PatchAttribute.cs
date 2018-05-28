namespace ContractHttp
{
     /// <summary>
    /// An attribute used to state that a method call should build a patch request.
    /// </summary>
   public class PatchAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="PatchAttribute"/> class.
        /// </summary>
        public PatchAttribute()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="PatchAttribute"/> class.
        /// </summary>
        /// <param name="template">A path template.</param>
        public PatchAttribute(string template)
        {
            this.Template = template;
        }
    }
}