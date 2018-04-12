namespace ContractHttpTests.Resources.Models
{
    using System.Net.Http;

    public class TestData
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public HttpResponseMessage Response { get; set;}
    }
}