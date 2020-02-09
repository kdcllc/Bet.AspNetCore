using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bet.Extensions.Hosting.Hosted
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly IEnumerable<ITimedHostedLifeCycleHook> _lifeCycleHooks;
        private readonly CancellationTokenSource _cancellationCts;
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly TimedHostedServiceOptions _options;
        private readonly IServiceProvider _serviceProvider;

        private Timer? _timer;

        public TimedHostedService(
            IServiceProvider serviceProvider,
            TimedHostedServiceOptions options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<TimedHostedService> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lifeCycleHooks = lifeCycleHooks ?? throw new ArgumentNullException(nameof(lifeCycleHooks));

            _cancellationCts = new CancellationTokenSource();

            _semaphoreSlim = new SemaphoreSlim(1);
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            var stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationCts.Token);

            _logger.LogWarning("The {serviceName} background thread is starting.", nameof(TimedHostedService));

            stoppingCts.Token.ThrowIfCancellationRequested();

            foreach (var lifeCycleHook in _lifeCycleHooks)
            {
                await lifeCycleHook.OnStartAsync(stoppingCts.Token).ConfigureAwait(false);
            }

            _timer = new Timer(async (_) => await ExecuteAsync(stoppingCts.Token), null, TimeSpan.Zero, _options.Interval);

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

                using var scope = _serviceProvider.CreateScope();
                await _options.TaskToExecuteAsync(_options, scope.ServiceProvider, cancellationToken);
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
            catch (Exception ex) when (_options.FailMode != FailMode.Unhandled)
            {
                _logger.LogWarning(ex, $"Exception occurred: {ex.Message}");

                if (_options.FailMode == FailMode.LogAndRetry)
                {
                    _timer?.Change(Timeout.InfiniteTimeSpan, _options.RetryInterval);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to execute the task", ex);
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
