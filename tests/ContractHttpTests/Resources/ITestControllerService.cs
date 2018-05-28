namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Defines a test controller.
    /// </summary>
    [HttpController(ControllerTypeName = "DataController", RoutePrefix = "api/data")]
    public interface ITestControllerService
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>A list of <see cref="TestData"/> instance.</returns>
        [HttpGet("")]
        IEnumerable<TestData> GetAll();

        /// <summary>
        /// Gets by name.
        /// </summary>
        /// <param name="name">The name of the data to return.</param>
        /// <returns>A <see cref="TestData"/> instance if found; otherwise null.</returns>
        [HttpGet("{name}")]
        TestData GetByName(string name);

        /// <summary>
        /// Delete by name.
        /// </summary>
        /// <param name="name">The name of the data to delete.</param>
        [HttpDelete("{name}")]
        [ServiceCallFilterTest]
        void Delete(string name);
    }
}