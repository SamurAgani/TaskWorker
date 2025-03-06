using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Administrative.CLI
{
    public class Program
    {
        public static string servicePath
        {
            get { return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Infrastructure\bin\Debug\net8.0\Infrastructure.exe")); }
            set { servicePath = value; }
        }
        public static async Task Main(string[] args)
        {

            while (true)
            {
                if (args.Length == 0)
                {
                    Console.Write("CLI> ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("No command entered. Type 'help' for usage.");
                        continue;
                    }
                    args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                }

                var command = args[0].ToLower();
                switch (command)
                {
                    case "help":
                        PrintHelp();
                        break;

                    case "add":
                        var name = args.Length > 1 ? args[1] : "UnnamedWorker";
                        await AddWorker(name);
                        break;

                    case "remove":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Usage: remove <workerId>");
                        }
                        else
                        {
                            await RemoveWorker(args[1]);
                        }
                        break;

                    case "execute":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: execute <workerId> <cmd>");
                        }
                        else
                        {
                            var wId = args[1];
                            var cmdLine = string.Join(" ", args.Skip(2));
                            await EnqueueTask(wId, cmdLine);
                        }
                        break;

                    case "status":
                        if (args.Length == 2)
                            await Status(args[1]);
                        else
                            await Status(null);
                        break;

                    case "service":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Usage: service <start|stop|status>");
                        }
                        else
                        {
                            var subCmd = args[1].ToLower();
                            switch (subCmd)
                            {
                                case "start":
                                    ServiceStart();
                                    break;
                                case "stop":
                                    await ServiceStop();
                                    break;
                                case "status":
                                    await ServiceStatus();
                                    break;
                                default:
                                    Console.WriteLine("Unknown service command.");
                                    break;
                            }
                        }
                        break;

                    case "exit":
                        return;

                    default:
                        PrintHelp();
                        break;
                }

                args = Array.Empty<string>();
            }
        }

        private static async Task AddWorker(string name)
        {
            var cmd = $"ADD_WORKER|{name}";
            Console.WriteLine("Server request:" + cmd);
            var response = await NamedPipeAdminClient.SendCommandAsync(cmd);

            Console.WriteLine("Server response:");
            Console.WriteLine(response);
        }

        private static async Task RemoveWorker(string workerId)
        {
            var cmd = $"REMOVE_WORKER|{workerId}";
            var response = await NamedPipeAdminClient.SendCommandAsync(cmd);

            Console.WriteLine("Server response:");
            Console.WriteLine(response);
        }

        private static async Task EnqueueTask(string workerId, string commandLine)
        {
            var cmd = $"ENQUEUE_TASK|{workerId}|{commandLine}";
            var response = await NamedPipeAdminClient.SendCommandAsync(cmd);

            Console.WriteLine("Server response:");
            Console.WriteLine(response);
        }

        private static async Task Status(string? workerId)
        {
            var cmd = workerId == null ? "STATUS" : $"STATUS|{workerId}";
            var response = await NamedPipeAdminClient.SendCommandAsync(cmd);

            Console.WriteLine("Server response:");
            Console.WriteLine(response);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  add <WorkerName>           - Add a new worker (server returns a new Worker ID)");
            Console.WriteLine("  remove <WorkerId>          - Remove a worker");
            Console.WriteLine("  execute <WorkerId> <cmd>   - Enqueue a command for a specific worker");
            Console.WriteLine("  status [WorkerId]          - Show status of all or one worker");
            Console.WriteLine("  service <start|stop|status> - Manage the service");
            Console.WriteLine("  help                       - Show this help text");
            Console.WriteLine("  exit                       - Exit the CLI");
        }

        private static void ServiceStart()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = servicePath,
                    WorkingDirectory = Path.GetDirectoryName(servicePath) ?? "",
                    UseShellExecute = true
                });

                Console.WriteLine($"Spawned new worker process with ID {servicePath}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service start error: {ex.Message}");
            }
        }

        private static async Task ServiceStop()
        {
            var cmd = "STOP_SERVER";
            var response = await NamedPipeAdminClient.SendCommandAsync(cmd);
            Console.WriteLine("Server response:");
            Console.WriteLine(response);
        }

        private static async Task ServiceStatus()
        {
            var cmd = "STATUS";
            var response = await NamedPipeAdminClient.SendCommandAsync(cmd);
            Console.WriteLine("Server response:");
            Console.WriteLine(response);
        }
    }
}
