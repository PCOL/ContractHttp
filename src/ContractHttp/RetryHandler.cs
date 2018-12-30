namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A simple retry handler.`
    /// </summary>
    public class RetryHandler
    {
        /// <summary>
        /// The retry count.
        /// </summary>
        private int retryCount;

        /// <summary>
        /// The wait time.
        /// </summary>
        private TimeSpan waitTime;

        /// <summary>
        /// The maximum time to wait between retries.
        /// </summary>
        private TimeSpan maxWaitTime = TimeSpan.MaxValue;

        /// <summary>
        /// A value indicating whetjer or not to double the wait time on each retry.
        /// </summary>
        private bool doubleWaitTime;

        /// <summary>
        /// A list of exceptions that will trigger a retry.
        /// </summary>
        private List<Type> exceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryHandler"/> class.
        /// </summary>
        public RetryHandler()
        {
        }

        /// <summary>
        /// Sets an exception type to retry on.
        /// </summary>
        /// <typeparam name="T">The type of exception</typeparam>
        /// <returns>The <see cref="RetryHandler"/> instance.</returns>
        public RetryHandler RetryOnException<T>()
            where T : Exception
        {
            this.exceptions = this.exceptions ?? new List<Type>();
            this.exceptions.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Sets the number of retries.
        /// </summary>
        /// <param name="count">The number of retries.`</param>
        /// <returns>The <see cref="RetryHandler"/> instance.</returns>
        public RetryHandler RetryCount(int count)
        {
            this.retryCount = count;
            return this;
        }

        /// <summary>
        /// Sets the time to wait between retries.
        /// </summary>
        /// <param name="waitTime">The time to wait between retries.</param>
        /// <returns>The <see cref="RetryHandler"/> instance.</returns>
        public RetryHandler WaitTime(TimeSpan waitTime)
        {
            this.waitTime = waitTime;
            return this;
        }

        /// <summary>
        /// Sets the maximum wait time if the wait time is set to increase upon retry.
        /// </summary>
        /// <param name="maxWaitTime">Sets the maximum wait time.</param>
        /// <returns>The <see cref="RetryHandler"/> instance.</returns>
        public RetryHandler MaxWaitTime(TimeSpan maxWaitTime)
        {
            this.maxWaitTime = maxWaitTime;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether or not to double the wait time after each retry.
        /// </summary>
        /// <param name="value">True to double the wait time; otherwise false.</param>
        /// <returns>The <see cref="RetryHandler"/> instance.</returns>
        public RetryHandler DoubleWaitTimeOnRetry(bool value)
        {
            this.doubleWaitTime = value;
            return this;
        }

        /// <summary>
        /// Executes a function with retry.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="responseHandler">A response handler.</param>
        /// <returns>The resultasync of the operation.</returns>
        public async Task<T> RetryAsync<T>(Func<Task<T>> function, Func<T, bool> responseHandler)
        {
            T lastResult = default(T);
            Exception lastEx = null;

            int retry = 0;
            TimeSpan wait = this.waitTime;
            while (retry < this.retryCount)
            {
                try
                {
                    lastResult = await function().ConfigureAwait(false);
                    if (responseHandler(lastResult) == true)
                    {
                        return lastResult;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;

                    if (this.exceptions == null ||
                        this.exceptions.Contains(ex.GetType()) == false)
                    {
                        throw;
                    }
                }

                retry++;
                if (retry < this.retryCount)
                {
                    await Task.Delay(wait).ConfigureAwait(false);

                    if (this.doubleWaitTime == true)
                    {
                        wait = TimeSpan.FromTicks(wait.Ticks * 2);
                        if (wait > this.maxWaitTime)
                        {
                            wait = this.maxWaitTime;
                        }
                    }
                }
            }

            if (lastEx != null)
            {
                throw lastEx;
            }

            return lastResult;
        }
    }
}