using System;
using System.Threading.Tasks;

namespace ContractHttp
{
    public class RetryHandler
    {
        private int retryCount;

        private int waitTime;

        private int maxWaitTime = int.MaxValue;

        private bool doubleWaitTime;

        public RetryHandler()
        {
        }

        public RetryHandler RetryCount(int count)
        {
            this.retryCount = count;
            return this;
        }

        public RetryHandler WaitTime(int waitTime)
        {
            this.waitTime = waitTime;
            return this;
        }

        public RetryHandler MaxWaitTime(int maxWaitTime)
        {
            this.maxWaitTime = maxWaitTime;
            return this;
        }

        public RetryHandler DoubleWaitTimeOnRetry(bool value)
        {
            this.doubleWaitTime = value;
            return this;
        }

        public async Task<T> RetryAsync<T>(Func<Task<T>> function, Func<T, bool> responseHandler)
        {
            int retry = 0;
            int wait = this.waitTime;;
            while (retry < this.retryCount)
            {
                try
                {
                    var result = await function();
                    if (responseHandler(result) == true)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                await Task.Delay(wait);

                if (this.doubleWaitTime == true)
                {
                    wait *= 2;
                    if (wait > this.maxWaitTime)
                    {
                        wait = this.maxWaitTime;
                    }
                }

                retry++;
            }

            return default(T); 
        }
    }
}