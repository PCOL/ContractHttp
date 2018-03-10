namespace ContractHttp
{
    using System;

    /// <summary>
    /// A return value attribute that allows the return value to be extracted from
    /// a model property.
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    public class FromModelAttribute
        : Attribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FromModelAttribute"/> class.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <param name="propertyName">The property name.</param>
        public FromModelAttribute(Type modelType, string propertyName)
        {
            this.ModelType = modelType;
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the model type.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName { get; }
    }
}