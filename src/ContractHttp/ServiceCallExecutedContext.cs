namespace ContractHttp
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Represents a service call executed context.
    /// </summary>
    public class ServiceCallExecutedContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCallExecutedContext"/> class.
        /// </summary>
        /// <param name="controller">The controller instance.</param>
        /// <param name="services">The services instance.</param>
        public ServiceCallExecutedContext(Controller controller, IServiceProvider services)
        {
            this.Controller = controller;
            this.Services = services;
        }

        /// <summary>
        /// Gets the controller instance.
        /// </summary>
        public Controller Controller { get; }

        /// <summary>
        /// Gets the services instance.
        /// </summary>
        public IServiceProvider Services { get; }
    }
}