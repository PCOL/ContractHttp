namespace ContractHttpTests
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Represents a test controll used for testing retry.
    /// </summary>
    [Route("api")]
    public class TestControllerForRetry
        : Controller
    {
        private TestRetryCounts retryCounts;

        /// <summary>
        /// Initialise a new instance of the <see cref="TestControllerForRetry"/> class.
        /// </summary>
        /// <param name="retryCounts">A type for collecting retry counts.</param>
        public TestControllerForRetry(TestRetryCounts retryCounts)
        {
            this.retryCounts = retryCounts;
        }

        /// <summary>
        /// Get.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
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