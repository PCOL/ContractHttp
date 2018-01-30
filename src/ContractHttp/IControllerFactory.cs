namespace ContractHttp
{
    using System;

    /// <summary>
    /// Defines the controller factory interface.
    /// </summary>
    public interface IControllerFactory
    {
        /// <summary>
        /// Creates a controller type.
        /// </summary>
        /// <typeparam name="T">The interface describing the controller type.</typeparam>
        /// <param name="controllerServiceType">The controllers implementation type.</param>
        /// <returns>A new controller type.</returns>
        Type CreateControllerType<T>(Type controllerServiceType);

        /// <summary>
        /// Create instance of a controller.
        /// </summary>
        /// <typeparam name="T">The interface type describing the controller.</typeparam>
        /// <param name="instance">The instance of the service the controlller will call into.</param>
        /// <returns>An instance of the controller.</returns>
        T CreateController<T>(object instance);

        /// <summary>
        /// Create instance of a controller.
        /// </summary>
        /// <param name="controllerType">The controller type</param>
        /// <param name="instance">The instance of the service the controlller will call into.</param>
        /// <returns>An instance of the controller.</returns>
        object CreateController(Type controllerType, object instance);
    }
}
