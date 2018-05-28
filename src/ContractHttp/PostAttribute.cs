namespace ContractHttp
{
    /// <summary>
    /// An attribute used to state that a method call should build a post request.
    /// </summary>
    public class PostAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostAttribute"/> class.
        /// </summary>
        public PostAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostAttribute"/> class.
        /// </summary>
        /// <param name="template">A path template.</param>
        public PostAttribute(string template)
        {
            this.Template = template;
        }
    }
}