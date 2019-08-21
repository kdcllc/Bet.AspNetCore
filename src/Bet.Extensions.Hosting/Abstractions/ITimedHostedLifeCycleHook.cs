using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.Hosting.Abstractions
{
    public interface ITimedHostedLifeCycleHook
    {
        Task OnStartAsync(CancellationToken cancellationToken);

        Task OnStopAsync(CancellationToken cancellationToken);

        Task OnRunOnceSucceededAsync(CancellationToken cancellationToken);

        Task OnExceptionAsync(Exception exception, CancellationToken cancellationToken);
    }
}
