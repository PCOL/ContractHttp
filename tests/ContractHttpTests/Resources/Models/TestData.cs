namespace ContractHttpTests.Resources.Models
{
    using System.Net.Http;

    /// <summary>
    /// Represents test data.
    /// </summary>
    public class TestData
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public TestModel Data { get; set; }

        /// <summary>
        /// Gets or sets the array.
        /// </summary>
        public TestModel[] Array { get; set; }

        /// <summary>
        /// Gets or sets the Http response.
        /// </summary>
        public HttpResponseMessage Response { get; set; }
    }
}