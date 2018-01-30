using ContractHttp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContractHttp.Tests
{
    [TestClass]
    public class HttpClientProxyUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateProxy_WithNullProxy_Throws()
        {
            new HttpClientProxy(null, null, null);
        }
    }
}
