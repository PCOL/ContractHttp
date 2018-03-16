namespace ContractHttp
{
    using System;
    using System.Net;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class RetryAttribute
        : Attribute
    {
        public HttpStatusCode[] HttpStatusCodeToRetry { get; set; }

        public int RetryCount { get; set; }

        public int WaitTime { get; set; }

        public int MaxWaitTime { get; set; }

        public bool DoubleWaitTimeOnRetry { get; set; }
    }
}