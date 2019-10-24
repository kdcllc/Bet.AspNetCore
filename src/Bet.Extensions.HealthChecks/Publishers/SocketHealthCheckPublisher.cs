using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Bet.Extensions.HealthChecks.Publishers
{
    public class SocketHealthCheckPublisher : IHealthCheckPublisher
    {
        private readonly Thread _thread;
        private readonly ILogger<SocketHealthCheckPublisher> _logger;

        public SocketHealthCheckPublisher(int port, ILogger<SocketHealthCheckPublisher> logger)
        {
            _thread = new Thread(() =>
            {
                var tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                StartListener(tcpListener);
            });

            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            try
            {
                if (report.Status == HealthStatus.Healthy)
                {
                    if (!_thread.IsAlive)
                    {
                        _thread.Start();
                    }
                }
                else
                {
                    if (_thread.IsAlive)
                    {
                        _thread.Abort();
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                _logger.LogCritical("Socket Thread is aborted: {code} with HealthCheck status {status}", ex.ExceptionState, report.Status);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Socket connection wasn't established: {message}", ex.Message);
            }

            await Task.CompletedTask;
        }

        private void StartListener(TcpListener tcpListener)
        {
            try
            {
                while (true)
                {
                    _logger.LogDebug("Waiting for a connection...");

                    var client = tcpListener.AcceptTcpClient();
                    _logger.LogDebug("Connected!");

                    var t = new Thread(new ParameterizedThreadStart(HandleConnection));
                    t.Start(client);
                }
            }
            catch (SocketException e)
            {
                _logger.LogCritical("SocketException terminated: {0}", e);
                tcpListener.Stop();
            }
        }

        private void HandleConnection(object obj)
        {
            var client = (TcpClient)obj;
            var stream = client.GetStream();
            var bytes = new byte[256];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    var hex = BitConverter.ToString(bytes);
                    var data = Encoding.ASCII.GetString(bytes, 0, i);

                    _logger.LogDebug("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);

                    var reply = Encoding.ASCII.GetBytes(data);
                    stream.Write(reply, 0, reply.Length);
                    _logger.LogDebug("{1}: Sent: {0}", data, Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: {0}", e.ToString());
                client.Close();
            }
        }
    }
}
