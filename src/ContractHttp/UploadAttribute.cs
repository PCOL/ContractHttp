namespace ContractHttp
{
    /// <summary>
    /// An attribute used to state that a method call should build an upload request.
    /// </summary>
    public class UploadAttribute
        : MethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttribute"/> class.
        /// </summary>
        /// <param name="template">A path template.</param>
        public UploadAttribute(string template)
        {
            this.Template = template;
        }
    }
}