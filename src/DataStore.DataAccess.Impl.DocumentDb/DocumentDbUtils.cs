namespace Finygo.DocumentDb
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Azure.Documents;

    using Serilog;

    public static class DocumentDbUtils
    {
        /// <summary>
        ///     Method that will detect "go slower" messages from DocDb and wait the correct time
        ///     It does not currently retry on network failures, though this really should be added
        /// </summary>
        /// <typeparam name="V">The return value of your lambda</typeparam>
        /// <param name="function">The lambda you want to run</param>
        /// <returns>The output of your operation</returns>
        // ReSharper disable once InconsistentNaming
        public static async Task<V> ExecuteWithRetries<V>(Func<Task<V>> function)
        {
            while (true)
            {
                TimeSpan sleepTime;
                try
                {
                    return await function().ConfigureAwait(false);
                }
                catch (DocumentClientException de)
                {
                    if (de.StatusCode != null && (int)de.StatusCode != 429)
                    {
                        Log.Logger.Error(
                            de, 
                            "A DocumentDbSettings operation failed with code {statusCode}. {error}", 
                            de.StatusCode, 
                            de.Message);
                        throw;
                    }

                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        throw;
                    }

                    var de = (DocumentClientException)ae.InnerException;
                    if (de.StatusCode != null && (int)de.StatusCode != 429)
                    {
                        Log.Logger.Error(
                            de, 
                            "A DocumentDbSettings operation failed with code {statusCode}. {error}", 
                            de.StatusCode, 
                            de.Message);
                        throw;
                    }

                    sleepTime = de.RetryAfter;
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "A DocumentDbSettings operation failed: {error}", e.Message);
                    throw new DatabaseException($"A DocumentDb operation failed: {e.Message}", e);
                }

                Log.Logger.Information("Got throttled, waiting {sleepTime:g}", sleepTime);
                await Task.Delay(sleepTime).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Method that will detect "go slower" messages from DocDb and wait the correct time
        ///     It does not currently retry on network failures, though this really should be added
        /// </summary>
        /// <typeparam name="V">The return value of your lambda</typeparam>
        /// <param name="function">The lambda you want to run</param>
        /// <returns>The output of your operation</returns>
        public static V ExecuteWithRetries<V>(Func<V> function)
        {
            while (true)
            {
                TimeSpan sleepTime;
                try
                {
                    return function();
                }
                catch (DocumentClientException de)
                {
                    if (de.StatusCode != null && (int)de.StatusCode != 429)
                    {
                        Log.Logger.Error(
                            de, 
                            "A DocumentDbSettings operation failed with code {statusCode}. {error}", 
                            de.StatusCode, 
                            de.Message);
                        throw;
                    }

                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        throw;
                    }

                    var de = (DocumentClientException)ae.InnerException;
                    if (de.StatusCode != null && (int)de.StatusCode != 429)
                    {
                        Log.Logger.Error(
                            de, 
                            "A DocumentDbSettings operation failed with code {statusCode}. {error}", 
                            de.StatusCode, 
                            de.Message);
                        throw;
                    }

                    sleepTime = de.RetryAfter;
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "A DocumentDbSettings operation failed: {error}", e.Message);
                    throw new DatabaseException($"A DocumentDb operation failed: {e.Message}", e);
                }

                Log.Logger.Information("Got throttled, waiting {sleepTime:g}", sleepTime);
                Thread.Sleep(sleepTime);
            }
        }
    }
}