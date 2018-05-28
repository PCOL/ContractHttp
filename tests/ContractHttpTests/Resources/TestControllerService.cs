namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
    using System.Net.Http;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// An implemtation of the <see cref="ITestControllerService"/> interface.
    /// </summary>
    public class TestControllerService
        : ITestControllerService
    {
        /// <summary>
        /// Get all.
        /// </summary>
        /// <returns>A list of <see cref="TestData"/> instances.</returns>
        public IEnumerable<TestData> GetAll()
        {
            return new[]
            {
                new TestData()
                {
                    Name = "Test1",
                    Address = "Somewhere1"
                },
                new TestData()
                {
                    Name = "Test2",
                    Address = "Somewhere2"
                }
            };
        }

        /// <summary>
        /// Get by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A <see cref="TestData"/> instance.</returns>
        public TestData GetByName(string name)
        {
            return new TestData()
            {
                Name = name,
                Address = "Somewhere"
            };
        }

        /// <summary>
        /// Deletes by name.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Delete(string name)
        {
        }
    }
}