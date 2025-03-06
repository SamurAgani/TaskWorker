namespace Domain.Entities
{
    public class TaskRequest
    {
        public Guid Id { get; private set; }
        public string CommandLine { get; private set; }
        public string? Result { get; set; } = "In Queue";
        public TaskRequest(string commandLine, Guid id, string result)
        {
            Id = id;
            CommandLine = commandLine;
            Result = result;
        }
        public TaskRequest(string commandLine)
        {
            Id = Guid.NewGuid();
            CommandLine = commandLine;
        }
    }
}
