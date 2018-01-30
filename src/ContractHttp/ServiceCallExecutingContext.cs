namespace ContractHttp
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Represents a service call executing context.
    /// </summary>
    public class ServiceCallExecutingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCallExecutingContext"/> class.
        /// </summary>
        /// <param name="controller">The controller instance,</param>
        /// <param name="services">The services instance.</param>
        public ServiceCallExecutingContext(Controller controller, IServiceProvider services)
        {
            this.Controller = controller;
            this.Services = services;
        }

        /// <summary>
        /// Gets the controller.
        /// </summary>
        public Controller Controller { get; }

        /// <summary>
        /// Gets the services.`
        /// </summary>
        public IServiceProvider Services { get; }
    }
}