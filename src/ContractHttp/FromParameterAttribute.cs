namespace Microsoft.AspNetCore.Http
{
    using System;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// Specifies that a parameter or property should be bound using a model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromParameterAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FromParameterAttribute"/> class.
        /// </summary>
        /// <param name="parameterName">The paremeters name.</param>
        /// <param name="parameterType">The perameters type.</param>
        public FromParameterAttribute(string parameterName, Type parameterType)
        {
            this.ParameterName = parameterName;
            this.ParameterType = parameterType;
        }

        /// <summary>
        /// Gets the name of the parameter that holds the model.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets the name of the parameter that holds the model.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets or sets the name of the property to bind to.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets the binding source.
        /// </summary>
        public BindingSource BindingSource { get; } = BindingSource.Custom;

        /// <summary>
        /// Gets or sets the converter.
        /// </summary>
        public Type ConverterType { get; set; }
    }
}