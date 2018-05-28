namespace ContractHttp
{
    /// <summary>
    /// An attribute used to state that a method call should build a put request.
    /// </summary>
    public class PutAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="PutAttribute"/> class.
        /// </summary>
        public PutAttribute()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="PutAttribute"/> class.
        /// </summary>
        /// <param name="template">A path template.</param>
        public PutAttribute(string template)
        {
            this.Template = template;
        }
    }
}