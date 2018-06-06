namespace ContractHttp
{
    using System;

    /// <summary>
    /// Represents a controller method parameter.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ControllerMethodParameterAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerMethodParameterAttribute"/> class.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="parameterType">The parameter type.</param>
        public ControllerMethodParameterAttribute(string parameterName, Type parameterType)
        {
            this.ParameterName = parameterName;
            this.ParameterType = parameterType;
        }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets or sets the from option.
        /// </summary>
        public ControllerMethodParameterFromOption From { get; set; }

        /// <summary>
        /// Gets or sets the from name.
        /// </summary>
        public string FromName { get; set; }
    }
}