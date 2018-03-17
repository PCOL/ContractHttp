using System;
using Microsoft.AspNetCore.Mvc;

namespace ContractHttpTests
{
    [Route("api")]
    public class TestControllerForRetry
        : Controller
    {
        private TestRetryCounts retryCounts;

        public TestControllerForRetry(TestRetryCounts retryCounts)
        {
            this.retryCounts = retryCounts;
        }

        [HttpGet("{status}")]
        public IActionResult Get(int status)
        {
            if (this.retryCounts.GetCount == 2)
            {
                return this.StatusCode(204);
            }

            this.retryCounts.GetCount++;
            return this.StatusCode(status);
        }
    }
}