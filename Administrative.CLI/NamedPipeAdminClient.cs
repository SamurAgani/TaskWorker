using System.IO.Pipes;

namespace Administrative.CLI
{
    public static class NamedPipeAdminClient
    {
        private const string ADMIN_PIPE = "MyAdminPipe";

        public static async Task<string> SendCommandAsync(string command)
        {
            StreamReader? reader = null;
            StreamWriter? writer = null;

            try
            {
                using var pipeClient = new NamedPipeClientStream(".", ADMIN_PIPE, PipeDirection.InOut, PipeOptions.None);
                await pipeClient.ConnectAsync(5000);

                writer = new StreamWriter(pipeClient, leaveOpen: true) { AutoFlush = true };
                reader = new StreamReader(pipeClient, leaveOpen: true);

                Console.WriteLine($"Sending command: {command}");
                try
                {
                    await writer.WriteLineAsync(command);
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"Write failed: {ioEx.Message}");
                    return string.Empty;
                }
                var lines = new List<string>();
                string? line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Trim().Equals("END", StringComparison.OrdinalIgnoreCase))
                        break;
                    lines.Add(line);
                }

                var response = string.Join(Environment.NewLine, lines);

                return response.Trim();
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
            finally
            {
                if (writer != null)
                {
                    try { writer.Dispose(); } catch {  }
                }
                if (reader != null)
                {
                    try { reader.Dispose(); } catch {  }
                }
            }
        }
    }
}
