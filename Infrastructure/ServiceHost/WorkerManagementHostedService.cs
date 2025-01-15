using Application.Services;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ServiceHost
{
    internal class WorkerManagementHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private const string ADMIN_PIPE = "MyAdminPipe";

        public WorkerManagementHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

                    //pipeServer.SetAccessControl(pipeSecurity);
                    await pipeServer.WaitForConnectionAsync(token);
                    _ = Task.Run(() => HandleAdminConnection(pipeServer, token), token);

                    //while (!token.IsCancellationRequested && pipeServer.IsConnected)
                    //{
                    //    await HandleAdminConnection(pipeServer, token);
                    //}
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
                var parts = request.Split('|');
                var cmd = parts[0];

                using var scope = _serviceProvider.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IWorkerManagementService>();

                switch (cmd)
                {
                    case "ADD_WORKER":
                        {
                            var name = parts.Length > 1 ? parts[1] : "UnnamedWorker";
                            var worker = svc.AddWorker(name);

                            var workerExePath = @"C:\Users\user\Desktop\JustTestMyself\TaskWorker\Worker\bin\Debug\net8.0\Worker.exe";
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = workerExePath,
                                Arguments = worker.Id.ToString(),
                                UseShellExecute = true,
                                WorkingDirectory = Path.GetDirectoryName(workerExePath) ?? ""
                            });
                            await writer.WriteLineAsync($"OK|{worker.Id} END");
                            break;
                        }

                    case "REMOVE_WORKER":
                        {
                            if (parts.Length > 1 && Guid.TryParse(parts[1], out var wId))
                            {
                                svc.RemoveWorker(wId);
                                string pipeName = $"WorkerPipe-{wId}";
                                using var workerPipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                                await workerPipeClient.ConnectAsync(5000);
                                using var wWriter = new StreamWriter(workerPipeClient) { AutoFlush = true };
                                await wWriter.WriteLineAsync("STOP");
                                await writer.WriteLineAsync("OK");
                            }
                            else
                            {
                                await writer.WriteLineAsync("ERROR|Invalid workerId");
                            }
                            break;
                        }

                    case "ENQUEUE_TASK":
                        {
                            if (parts.Length > 2 && Guid.TryParse(parts[1], out var wId))
                            {

                                StreamReader? wReader = null;
                                StreamWriter? wWriter = null;
                                var cmdLine = parts[2];
                                var taskId = svc.EnqueueTask(wId, cmdLine);
                                string pipeName = $"WorkerPipe-{wId}";
                                using var workerPipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                                await workerPipeClient.ConnectAsync(5000);
                                wWriter = new StreamWriter(workerPipeClient, leaveOpen: true) { AutoFlush = true };
                                wReader = new StreamReader(workerPipeClient, leaveOpen: true);

                                await wWriter.WriteLineAsync($"EXECUTE|{taskId}|{cmdLine}");
                                var result = await wReader.ReadLineAsync();
                                svc.UpdateTaskResult(wId, taskId, result ?? string.Empty);
                                var a = svc.GetWorkerById(wId);
                                await writer.WriteLineAsync($"OK|Task executed with result: {result}");
                            }
                            else
                            {
                                await writer.WriteLineAsync("ERROR|Invalid arguments");
                            }
                            break;
                        }

                    case "STOP_SERVER":
                        {
                            var allWorkers = svc.GetAllWorkers();
                            foreach (var w in allWorkers)
                            {
                                string pipeName = $"WorkerPipe-{w.Id}";
                                try
                                {
                                    using var workerPipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                                    await workerPipeClient.ConnectAsync(5000);
                                    using var wWriter = new StreamWriter(workerPipeClient) { AutoFlush = true };
                                    await wWriter.WriteLineAsync("STOP");
                                }
                                catch
                                {
                                }
                            }
                            await writer.WriteLineAsync("OK|Stopping server.");
                            Environment.Exit(0);
                            break;
                        }

                    case "STATUS":
                        {
                            if (parts.Length > 1 && Guid.TryParse(parts[1], out var workerId))
                            {
                                var worker = svc.GetWorkerById(workerId);
                                if (worker == null)
                                {
                                    await writer.WriteLineAsync("ERROR|Worker not found");
                                }
                                else
                                {
                                    await writer.WriteLineAsync($"OK|Single|{worker.Id}|{worker.Name}|{worker.IsActive}|{worker.Tasks.Count}");
                                }
                                await writer.WriteLineAsync("END");
                            }
                            else
                            {
                                var allWorkers = svc.GetAllWorkers();
                                string result = $"OK|All|{allWorkers.Count} \n";
                                foreach (var w in allWorkers)
                                {
                                    result += $"Id:{w.Id}| Name:{w.Name}| Is active:{w.IsActive}| TaskCount:{w.Tasks.Count}\n";
                                    foreach(var t in w.Tasks)
                                    {
                                        result += $"TaskCommand: {t.CommandLine}| Result: {t.Result}\n";
                                    }
                                }
                                await writer.WriteLineAsync($"{result} END");
                            }
                            break;
                        }

                    default:
                        await writer.WriteLineAsync("ERROR|Unknown admin command");
                        break;

                }
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