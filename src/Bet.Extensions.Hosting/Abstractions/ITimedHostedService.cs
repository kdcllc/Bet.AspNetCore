using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bet.Extensions.Hosting.Abstractions
{
    /// <summary>
    /// Timed hosted service.
    /// </summary>
    public interface ITimedHostedService : IHostedService, IDisposable
    {
        TimedHostedServiceOptions Options { get; }

        ILogger<ITimedHostedService> Logger { get; }

        Func<CancellationToken, Task> TaskToExecuteAsync { get; set; }

        Task ExecuteOnceAsync(CancellationToken cancellationToken);

        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
