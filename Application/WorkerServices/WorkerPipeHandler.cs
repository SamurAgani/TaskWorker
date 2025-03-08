using System.IO.Pipes;

namespace Application.WorkerServices
{
    public class WorkerPipeHandler : IWorkerPipeHandler
    {
        private readonly IWorkerCommandExecutor _commandExecutor;
        public WorkerPipeHandler(IWorkerCommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }
        public async Task HandleConnectionAsync(NamedPipeServerStream pipeServer)
        {
            try
            {
                using var reader = new StreamReader(pipeServer);
                using var writer = new StreamWriter(pipeServer) { AutoFlush = true };
                var request = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(request))
                    return;
                var parts = request.Split('|');
                var command = parts[0];
                if (command == "STOP")
                {
                    Console.WriteLine("Received stop command. Exiting.");
                    pipeServer.Disconnect();
                    Environment.Exit(0);
                }
                else if (command == "EXECUTE" && parts.Length >= 3)
                {
                    var taskId = parts[1];
                    var cmdLine = parts[2];
                    Console.WriteLine($"Received EXECUTE: Task {taskId}, Command: {cmdLine}");
                    string result = await Task.Run(() => _commandExecutor.ExecuteCommand(cmdLine));
                    await writer.WriteLineAsync(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Worker pipe error: {ex.Message}");
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