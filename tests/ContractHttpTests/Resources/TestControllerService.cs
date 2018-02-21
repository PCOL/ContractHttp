namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
    using ContractHttpTests.Resources.Models;

    public class TestControllerService
        : ITestControllerService
    {
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

        public TestData GetByName(string name)
        {
            return new TestData()
            {
                Name = name,
                Address = "Somewhere"
            };
        }
    }
}