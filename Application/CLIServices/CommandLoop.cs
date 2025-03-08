
namespace Application.CLIServices
{
    public class CommandLoop : ICommandLoop
    {
        private readonly IWorkerAdminService _workerService;
        private readonly IServiceManager _serviceManager;
        public CommandLoop(IWorkerAdminService workerService, IServiceManager serviceManager)
        {
            _workerService = workerService;
            _serviceManager = serviceManager;
        }
        public async Task RunAsync(string[] args)
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
                        {
                            var name = args.Length > 1 ? args[1] : "UnnamedWorker";
                            var response = await _workerService.AddWorkerAsync(name);
                            Console.WriteLine("Server response:");
                            Console.WriteLine(response);
                        }
                        break;
                    case "remove":
                        {
                            if (args.Length < 2)
                            {
                                Console.WriteLine("Usage: remove <workerId>");
                            }
                            else
                            {
                                var response = await _workerService.RemoveWorkerAsync(args[1]);
                                Console.WriteLine("Server response:");
                                Console.WriteLine(response);
                            }
                        }
                        break;
                    case "execute":
                        {
                            if (args.Length < 3)
                            {
                                Console.WriteLine("Usage: execute <workerId> <cmd>");
                            }
                            else
                            {
                                var workerId = args[1];
                                var cmdLine = string.Join(" ", args.Skip(2));
                                var response = await _workerService.EnqueueTaskAsync(workerId, cmdLine);
                                Console.WriteLine("Server response:");
                                Console.WriteLine(response);
                            }
                        }
                        break;
                    case "status":
                        {
                            string? workerId = args.Length == 2 ? args[1] : null;
                            var response = await _workerService.GetStatusAsync(workerId);
                            Console.WriteLine("Server response:");
                            Console.WriteLine(response);
                        }
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "service":
                        {
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
                                        _serviceManager.StartService();
                                        Console.WriteLine($"Spawned new worker process with ID {_serviceManager}.");
                                        break;
                                    case "stop":
                                        {
                                            var response = await _serviceManager.StopServiceAsync();
                                            Console.WriteLine("Server response:");
                                            Console.WriteLine(response);
                                        }
                                        break;
                                    case "status":
                                        {
                                            var response = await _serviceManager.GetServiceStatusAsync();
                                            Console.WriteLine("Server response:");
                                            Console.WriteLine(response);
                                        }
                                        break;
                                    default:
                                        Console.WriteLine("Unknown service command.");
                                        break;
                                }
                            }
                        }
                        break;
                    case "exit":
                        {
                            var response = await _serviceManager.StopServiceAsync();
                            Console.WriteLine("Server response:");
                            Console.WriteLine(response);
                            return;
                        }
                    default:
                        PrintHelp();
                        break;
                }
                args = Array.Empty<string>();
            }
        }
        private void PrintHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  add <WorkerName>           - Add a new worker (server returns a new Worker ID)");
            Console.WriteLine("  remove <WorkerId>          - Remove a worker");
            Console.WriteLine("  execute <WorkerId> <cmd>   - Enqueue a command for a specific worker");
            Console.WriteLine("  status [WorkerId]          - Show status of all or one worker");
            Console.WriteLine("  service <start|stop|status> - Manage the service");
            Console.WriteLine("  help                       - Show this help text");
            Console.WriteLine("  clear                      - Cleaning the screen");
            Console.WriteLine("  exit                       - Exit the CLI");
        }
    }
}
