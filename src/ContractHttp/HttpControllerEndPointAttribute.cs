namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the atrributes of a http controller end point.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpControllerEndPointAttribute
        : Attribute
    {
        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the http method.
        /// </summary>
        public HttpCallMethod Method { get; set; }
    }
}
