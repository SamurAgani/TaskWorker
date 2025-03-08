using System.Diagnostics;
using System.IO.Pipes;

namespace Application.ServerServices
{
    public class ServerCommandHandler : IServerCommandHandler
    {
        private readonly IWorkerManagementService _svc;
        public ServerCommandHandler(IWorkerManagementService svc)
        {
            _svc = svc;
        }
        public async Task HandleAsync(string request, CancellationToken token, StreamWriter writer)
        {
            var parts = request.Split('|');
            var cmd = parts[0];
            switch (cmd)
            {
                case "ADD_WORKER":
                    await HandleAddWorker(parts, writer);
                    break;
                case "REMOVE_WORKER":
                    await HandleRemoveWorker(parts, writer);
                    break;
                case "ENQUEUE_TASK":
                    await HandleEnqueueTask(parts, writer);
                    break;
                case "STOP_SERVER":
                    await HandleStopServer(writer);
                    break;
                case "STATUS":
                    await HandleStatus(parts, writer);
                    break;
                default:
                    await writer.WriteLineAsync("ERROR|Unknown admin command");
                    break;
            }
        }
        private async Task HandleAddWorker(string[] parts, StreamWriter writer)
        {
            var name = parts.Length > 1 ? parts[1] : "UnnamedWorker";
            var worker = _svc.AddWorker(name);
            var workerExePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Worker\bin\Debug\net8.0\Worker.exe"));
            Process.Start(new ProcessStartInfo
            {
                FileName = workerExePath,
                Arguments = worker.Id.ToString(),
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(workerExePath) ?? ""
            });
            await writer.WriteLineAsync($"OK|{worker.Id} END");
        }
        private async Task HandleRemoveWorker(string[] parts, StreamWriter writer)
        {
            if (parts.Length > 1 && Guid.TryParse(parts[1], out var wId))
            {
                _svc.RemoveWorker(wId);
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
        }
        private async Task HandleEnqueueTask(string[] parts, StreamWriter writer)
        {
            if (parts.Length > 2 && Guid.TryParse(parts[1], out var wId))
            {
                var cmdLine = parts[2];
                var taskId = _svc.EnqueueTask(wId, cmdLine);
                string pipeName = $"WorkerPipe-{wId}";
                using var workerPipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await workerPipeClient.ConnectAsync(5000);
                using var wWriter = new StreamWriter(workerPipeClient, leaveOpen: true) { AutoFlush = true };
                using var wReader = new StreamReader(workerPipeClient, leaveOpen: true);
                await wWriter.WriteLineAsync($"EXECUTE|{taskId}|{cmdLine}");
                var result = await wReader.ReadLineAsync();
                _svc.UpdateTaskResult(wId, taskId, result ?? string.Empty);
                await writer.WriteLineAsync($"OK|Task executed with result: {result}");
            }
            else
            {
                await writer.WriteLineAsync("ERROR|Invalid arguments");
            }
        }
        private async Task HandleStopServer(StreamWriter writer)
        {
            var allWorkers = _svc.GetAllWorkers();
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
                catch { }
            }
            await writer.WriteLineAsync("OK|Stopping server.");
            Environment.Exit(0);
        }
        private async Task HandleStatus(string[] parts, StreamWriter writer)
        {
            if (parts.Length > 1 && Guid.TryParse(parts[1], out var workerId))
            {
                var worker = _svc.GetWorkerById(workerId);
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
                var allWorkers = _svc.GetAllWorkers();
                string result = $"OK|All|{allWorkers.Count} \n";
                foreach (var w in allWorkers)
                {
                    result += $"Id:{w.Id}| Name:{w.Name}| Is active:{w.IsActive}| TaskCount:{w.Tasks.Count}\n";
                    foreach (var t in w.Tasks)
                    {
                        result += $"TaskCommand: {t.CommandLine}| Result: {t.Result}\n";
                    }
                }
                await writer.WriteLineAsync($"{result} END");
            }
        }
    }
}