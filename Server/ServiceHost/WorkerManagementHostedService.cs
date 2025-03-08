using Microsoft.Extensions.Hosting;
using System.IO.Pipes;
using Application.ServerServices;

namespace Server.ServiceHost
{
    internal class WorkerManagementHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerCommandHandler _serviceCommandHandler;
        private const string ADMIN_PIPE = "MyAdminPipe";
        public WorkerManagementHostedService(IServiceProvider serviceProvider, IServerCommandHandler serviceCommandHandler)
        {
            _serviceProvider = serviceProvider;
            _serviceCommandHandler = serviceCommandHandler;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => AcceptAdminConnections(stoppingToken), stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        private async Task AcceptAdminConnections(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var pipeServer = new NamedPipeServerStream(
                        ADMIN_PIPE,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous,
                        0,
                        4096
                    );
                    await pipeServer.WaitForConnectionAsync(token);
                    _ = Task.Run(() => HandleAdminConnection(pipeServer, token), token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task HandleAdminConnection(NamedPipeServerStream pipeServer, CancellationToken token)
        {
            try
            {
                using var reader = new StreamReader(pipeServer);
                using var writer = new StreamWriter(pipeServer) { AutoFlush = true };
                var request = await reader.ReadLineAsync();
                Console.WriteLine(request);
                await _serviceCommandHandler.HandleAsync(request, token, writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in admin connection: " + ex.Message);
            }
            finally
            {
                if (pipeServer.IsConnected)
                    pipeServer.Disconnect();
                pipeServer.Dispose();
            }
        }
    }
}
