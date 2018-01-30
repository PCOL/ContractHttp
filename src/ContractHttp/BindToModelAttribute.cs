namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class BindToModelAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBinderAttribute"/> class.
        /// </summary>
        /// <param name="modelType">The model type</param>
        public BindToModelAttribute(Type modelType)
        {
            this.ModelType = modelType;
        }

        /// <summary>
        /// Gets the model type.
        /// </summary>
        public Type ModelType { get; }
    }
}