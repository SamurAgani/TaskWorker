# Task Worker System

This system consists of three main components:

- **Service Application**: Manages worker processes, tasks, and communication.
- **Administrative CLI**: Provides a command-line interface to interact with the service.
- **Worker Application**: Connects to the service, executes tasks, and returns results.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed.
- Administrative privileges for running certain commands and services.

## Setup Instructions

### 1. Build the Projects

Build all three projects: Service, Administrative.CLI, and Worker.

Using the .NET CLI:

```bash
dotnet build
```

Or open the solution in Visual Studio and build the entire solution.

### 2. Run Administrative CLI

After updating paths and building the projects, run the Administrative CLI as an administrator:

```bash
Path to the your AdmistrativeCli + \Administrative.CLI\bin\Debug\net8.0\Administrative.CLI.exe
```

## Usage Instructions

Once the Administrative CLI is running, use the following commands:

### Start the Service

```plaintext
service start
```
Starts the service application.

### Add a New Worker

```plaintext
add <WorkerName>
```
Creates a new worker instance with the given name (no spaces allowed) and returns a worker ID.

### Execute a Command on a Worker

```plaintext
execute <WorkerId> <command>
```
Enqueues a command for the specified worker. Returns the result of the command execution.

### Check Status of Workers

- **For all workers:**

  ```plaintext
  status
  ```
  Displays all workers, their current and previous tasks with results, and how many jobs are queued for each.

- **For a specific worker:**

  ```plaintext
  status <WorkerId>
  ```
  Displays the status of the specified worker.

### Remove a Worker

```plaintext
remove <WorkerId>
```
Closes the specified worker instance.

### Stop the Service

```plaintext
service stop
```
Stops the service application.

## Example Session

```plaintext
C:\Windows\System32>C:\Users\user\Desktop\JustTestMyself\TaskWorker\Administrative.CLI\bin\Debug\net8.0\Administrative.CLI.exe
CLI> help
Commands:
  add <WorkerName>           - Add a new worker (server returns a new Worker ID)
  remove <WorkerId>          - Remove a worker
  execute <WorkerId> <cmd>   - Enqueue a command for a specific worker
  status [WorkerId]          - Show status of all or one worker
  service <start|stop|status> - Manage the service
  help                       - Show this help text
  exit                       - Exit the CLI

CLI> service start
Spawned new worker process with ID C:\Users\user\Desktop\JustTestMyself\TaskWorker\Infrastructure\bin\Debug\net8.0\Infrastructure.exe.

CLI> add newworker
Server request:ADD_WORKER|newworker
Sending command: ADD_WORKER|newworker
Server response:
OK|8bc1e677-cdd5-4c71-b375-4a7a66eafbd6 END

CLI> execute 8bc1e677-cdd5-4c71-b375-4a7a66eafbd6 whoami
Sending command: ENQUEUE_TASK|8bc1e677-cdd5-4c71-b375-4a7a66eafbd6|whoami
Server response:
OK|Task executed with result: win-e5inkajm1mq\user\n

CLI> status
Sending command: STATUS
Server response:
OK|All|1
Id:8bc1e677-cdd5-4c71-b375-4a7a66eafbd6| Name:newworker| Is active:True| TaskCount:1
TaskCommand: whoami| Result: win-e5inkajm1mq\user\n

CLI> status 8bc1e677-cdd5-4c71-b375-4a7a66eafbd6
Sending command: STATUS|8bc1e677-cdd5-4c71-b375-4a7a66eafbd6
Server response:
OK|Single|8bc1e677-cdd5-4c71-b375-4a7a66eafbd6|newworker|True|1

CLI> remove 8bc1e677-cdd5-4c71-b375-4a7a66eafbd6
Sending command: REMOVE_WORKER|8bc1e677-cdd5-4c71-b375-4a7a66eafbd6
Server response:
OK

CLI> service stop
Sending command: STOP_SERVER
Server response:
OK|Stopping server.
```

## Notes

- Executing "help" in CLI will show you all commands.
- Executing "service start" one time will be enough for handling all workers and CLI calls.
- Update the file paths in the code as described before building.
- Running the Administrative CLI as an administrator is recommended for service management.
- The status command provides details about workers and their tasks.
- The system supports concurrent operations, allowing multiple CLI instances to interact with the service and workers simultaneously.
- The response reading mechanism expects an `END` marker from the server to properly terminate multi-line responses. Ensure the server sends `END` at the end of responses where needed.
