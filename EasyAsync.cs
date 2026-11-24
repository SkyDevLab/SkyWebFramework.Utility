using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Asynchronous resilience, retry, timeout, and concurrency helpers.
    /// </summary>
    public static class EasyAsync
    {
        /// <summary>
        /// Executes an asynchronous operation with configurable retries and optional exponential backoff.
        /// </summary>
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            TimeSpan? initialDelay = null,
            bool exponentialBackoff = true,
            Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));
            if (maxRetries < 0) throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries cannot be negative.");

            var delay = initialDelay ?? TimeSpan.FromMilliseconds(200);
            int attempt = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException && attempt < maxRetries && (shouldRetry is null || shouldRetry(ex)))
                {
                    attempt++;
                    var currentDelay = exponentialBackoff
                        ? TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt - 1))
                        : delay;

                    if (currentDelay > TimeSpan.Zero)
                        await Task.Delay(currentDelay, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Executes a void asynchronous operation with configurable retries and optional exponential backoff.
        /// </summary>
        public static async Task RetryAsync(
            Func<Task> operation,
            int maxRetries = 3,
            TimeSpan? initialDelay = null,
            bool exponentialBackoff = true,
            Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));

            await RetryAsync(async () =>
            {
                await operation().ConfigureAwait(false);
                return true;
            }, maxRetries, initialDelay, exponentialBackoff, shouldRetry, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Applies a timeout to a <see cref="Task{TResult}"/>, throwing <see cref="TimeoutException"/> if it exceeds the duration.
        /// </summary>
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (task is null) throw new ArgumentNullException(nameof(task));
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var delayTask = Task.Delay(timeout, cts.Token);
            var completed = await Task.WhenAny(task, delayTask).ConfigureAwait(false);

            if (completed == task)
            {
                cts.Cancel();
                return await task.ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException($"The operation timed out after {timeout.TotalMilliseconds:F0} ms.");
        }

        /// <summary>
        /// Applies a timeout to a <see cref="Task"/>, throwing <see cref="TimeoutException"/> if it exceeds the duration.
        /// </summary>
        public static async Task WithTimeout(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (task is null) throw new ArgumentNullException(nameof(task));
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var delayTask = Task.Delay(timeout, cts.Token);
            var completed = await Task.WhenAny(task, delayTask).ConfigureAwait(false);

            if (completed == task)
            {
                cts.Cancel();
                await task.ConfigureAwait(false);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException($"The operation timed out after {timeout.TotalMilliseconds:F0} ms.");
        }

        /// <summary>
        /// Safely observes exceptions from a fire-and-forget task without crashing the synchronization context.
        /// </summary>
        public static async void SafeFireAndForget(this Task task, Action<Exception>? onException = null)
        {
            if (task is null) return;
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }
        }

        /// <summary>
        /// Executes a collection of task factories concurrently while bounding maximum parallelism.
        /// </summary>
        public static async Task<IReadOnlyList<T>> WhenAllLimited<T>(
            IEnumerable<Func<Task<T>>> taskFactories,
            int maxConcurrency,
            CancellationToken cancellationToken = default)
        {
            if (taskFactories is null) throw new ArgumentNullException(nameof(taskFactories));
            if (maxConcurrency <= 0) throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Max concurrency must be at least 1.");

            using var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task<T>>();

            foreach (var factory in taskFactories)
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await factory().ConfigureAwait(false);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a collection of void task factories concurrently while bounding maximum parallelism.
        /// </summary>
        public static async Task WhenAllLimited(
            IEnumerable<Func<Task>> taskFactories,
            int maxConcurrency,
            CancellationToken cancellationToken = default)
        {
            if (taskFactories is null) throw new ArgumentNullException(nameof(taskFactories));
            if (maxConcurrency <= 0) throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Max concurrency must be at least 1.");

            using var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            foreach (var factory in taskFactories)
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await factory().ConfigureAwait(false);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
