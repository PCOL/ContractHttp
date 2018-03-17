namespace ContractHttpTests
{
    using System;
    using System.Threading.Tasks;
    using ContractHttp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RetryHandlerUnitTests
    {
        [TestMethod]
        public void RetryHandler_ResponseHandelerReturnsFalse_RetriesThreeTimes()
        {
            int count =0;
            var result = new RetryHandler()
                .RetryCount(3)
                .RetryAsync<bool>(
                    () =>
                    {
                        count++;
                        return Task.FromResult(true);
                    },
                    (r) =>
                    {
                        return false;
                    });

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void RetryHandler_RetryOnException_ThenThrowException()
        {
            int count = 0;
            Exception thrownEx = null;

            try
            {
                var result = new RetryHandler()
                    .RetryCount(3)
                    .RetryOnException<InvalidOperationException>()
                    .RetryAsync<bool>(
                        () =>
                        {
                            count++;
                            throw new InvalidOperationException();
                        },
                        (r) =>
                        {
                            return false;
                        }).Result;
            }
            catch (AggregateException ex)
            {
                thrownEx = ex.Flatten().InnerException;
            }

            Assert.AreEqual(3, count);
            Assert.AreEqual(typeof(InvalidOperationException), thrownEx.GetType());
        }

        [TestMethod]
        public void RetryHandler_RetryOnException_ThenReturnSuccess()
        {
            int count = 0;
            var result = new RetryHandler()
                .RetryCount(3)
                .RetryOnException<InvalidOperationException>()
                .RetryAsync<bool>(
                    () =>
                    {
                        count++;
                        if (count < 2)
                        {
                            throw new InvalidOperationException();
                        }

                        return Task.FromResult(true);
                    },
                    (r) => r).Result;

            Assert.AreEqual(2, count);
            Assert.IsTrue(result);
        }
    }
}