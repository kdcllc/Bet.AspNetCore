using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Bet.Extensions.HealthChecks.Publishers
{
    /// <summary>
    /// The health check publishers are run on a schedule and provide a convenient entry point.
    /// </summary>
    public class SocketHealthCheckPublisher : IHealthCheckPublisher
    {
        private readonly TcpListener _listener;
        private readonly ILogger<SocketHealthCheckPublisher> _logger;
        private int _started;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketHealthCheckPublisher"/> class.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="logger"></param>
        public SocketHealthCheckPublisher(int port, ILogger<SocketHealthCheckPublisher> logger)
        {
            _listener = new TcpListener(IPAddress.Any, port);

            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            try
            {
                if (report.Status == HealthStatus.Healthy)
                {
                    if (_started == 0)
                    {
                        await ListenAsync();
                    }
                }
                else
                {
                    _listener.Stop();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Something went wrong with the Socket connection: {message}", ex.Message);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Task.CompletedTask;
        }

        private async Task ListenAsync()
        {
            try
            {
                Interlocked.Increment(ref _started);

                _listener.Start();

                _logger.LogInformation("Waiting for a connection...");

                // Continue listening.
                TcpClient? client = null;
                while ((client = await _listener.AcceptTcpClientAsync()) != null
                    && client.Connected)
                {
                    _logger.LogInformation("[TcpSocket HealthCheck] Client Connected...");

                    await ProcessAsync(client);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("[TcpSocket HealthCheck] failed: {0}", ex);
                _listener.Stop();

                // reset count
                Interlocked.Decrement(ref _started);
            }

            await Task.CompletedTask;
        }

        private async Task ProcessAsync(TcpClient client)
        {
            var bytes = new byte[1024];
            int i;
            try
            {
                using var stream = client.GetStream();
                while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                {
                    // received.
                    var data = bytes.ConvertToString(0, i);
                    _logger.LogInformation("{threadId}: Data Received: {data}", Thread.CurrentThread.ManagedThreadId, data);

                    // send
                    var reply = data.ToBytes();
                    await stream.WriteAsync(reply, 0, reply.Length);
                    _logger.LogInformation("{threadId}: Data Sent: {data}", Thread.CurrentThread.ManagedThreadId, data);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("ProcessAsync failed with : {message}", e.GetBaseException().Message);
                client.Close();
            }
        }
    }
}
