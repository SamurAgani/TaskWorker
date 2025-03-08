using System.Diagnostics;

namespace Application.WorkerServices
{
    public class WorkerCommandExecutor : IWorkerCommandExecutor
    {
        public string ExecuteCommand(string command)
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
                using var proc = Process.Start(psi);
               // Task.Delay(5000).Wait();
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