namespace ContractHttp
{
    using System;

    /// <summary>
    /// Defines a generated http controller.
    /// </summary>
    public class HttpControllerAttribute
        : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the controller.
        /// </summary>
        public string ControllerTypeName { get; set; }

        /// <summary>
        /// Gets or sets the controller route prefix.
        /// </summary>
        public string RoutePrefix { get; set; }
    }
}
