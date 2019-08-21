using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.Hosting.Abstractions
{
    public abstract class TimedHostedService : ITimedHostedService
    {
        private readonly IEnumerable<ITimedHostedLifeCycleHook> _lifeCycleHooks;
        private readonly CancellationTokenSource _cancellationCts;
        private readonly SemaphoreSlim _semaphoreSlim;
        private Timer _timer;

        protected TimedHostedService(
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger)
        {
            Options = options.CurrentValue;

            options.OnChange(changedOptions => Options = changedOptions);

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lifeCycleHooks = lifeCycleHooks;
            _cancellationCts = new CancellationTokenSource();
            _semaphoreSlim = new SemaphoreSlim(1);
        }

        public TimedHostedServiceOptions Options { get; private set; }

        public ILogger<ITimedHostedService> Logger { get; }

        public Func<CancellationToken, Task> TaskToExecuteAsync { get; set; }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            var stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationCts.Token);

            Logger.LogWarning("The {serviceName} background thread is starting.", nameof(TimedHostedService));

            stoppingCts.Token.ThrowIfCancellationRequested();

            foreach (var lifeCycleHook in _lifeCycleHooks)
            {
                await lifeCycleHook.OnStartAsync(stoppingCts.Token).ConfigureAwait(false);
            }

            _timer = new Timer(async (_) => await ExecuteAsync(stoppingCts.Token), null, TimeSpan.Zero, Options.Interval);

            await Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            var stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationCts.Token);

            _timer?.Change(Timeout.Infinite, 0);

            foreach (var lifeCycleHook in _lifeCycleHooks)
            {
                await lifeCycleHook.OnStopAsync(stoppingCts.Token);
            }

            // Signal cancellation to the executing method
            _cancellationCts.Cancel();
            stoppingCts?.Dispose();

            await Task.CompletedTask;
        }

        public virtual async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_semaphoreSlim.CurrentCount == 0)
                {
                    return;
                }

                await _semaphoreSlim.WaitAsync();

                await TaskToExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                foreach (var lifeCycleHook in _lifeCycleHooks)
                {
                    await lifeCycleHook.OnExceptionAsync(ex, cancellationToken);
                }

                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ExecuteOnceAsync(cancellationToken);

                foreach (var lifeCycleHook in _lifeCycleHooks)
                {
                    await lifeCycleHook.OnRunOnceSucceededAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (Options.FailMode != FailMode.Unhandled)
            {
                if (Options.FailMode == FailMode.LogAndRetry)
                {
                    Logger.LogWarning(ex, $"Exception occurred: {ex.Message} - Retrying");
                    _timer.Change(Timeout.InfiniteTimeSpan, Options.RetryInterval);
                }
                else
                {
                    Logger.LogWarning(ex, $"Exception occurred: {ex.Message} - Continue");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();

                _cancellationCts?.Cancel();
                _cancellationCts?.Dispose();

                _semaphoreSlim?.Dispose();
            }
        }
    }
}
