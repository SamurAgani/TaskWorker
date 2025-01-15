using Domain;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Guid workerId = (args.Length > 0 && Guid.TryParse(args[0], out var parsed))
                ? parsed
                : Guid.NewGuid();

            string pipeName = $"WorkerPipe-{workerId}";
            Console.WriteLine($"Worker started. ID = {workerId}");

            while (true)
            {
                var pipeServer = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    0,
                    0);

                await pipeServer.WaitForConnectionAsync();
                _ = Task.Run(() => HandleConnectionAsync(pipeServer));
            }
        }

        private static async Task HandleConnectionAsync(NamedPipeServerStream pipeServer)
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
                    pipeServer.Dispose();
                    Environment.Exit(0);
                }
                else if (command == "EXECUTE" && parts.Length >= 3)
                {
                    var taskId = parts[1];
                    var cmdLine = parts[2];
                    Console.WriteLine($"Received EXECUTE: Task {taskId}, Command: {cmdLine}");

                    string result = await Task.Run(() => ExecuteCommand(cmdLine));
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
                {
                    pipeServer.Disconnect();
                }
                pipeServer.Dispose();
            }
        }

        private static string ExecuteCommand(string command)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                //Task.Delay(50000).RunSynchronously();
                using var proc = Process.Start(psi);
                proc.WaitForExit();

                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(error))
                    output += Environment.NewLine + "ERROR: " + error;

                return output.Replace("\r", "").Replace("\n", "\\n");
            }
            catch (Exception ex)
            {
                return $"EXEC_ERROR: {ex.Message}";
            }
        }
    }
}
