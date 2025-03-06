namespace Application.DTOs
{
    public class WorkerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<TaskRequestDto> Tasks { get; set; } = new();
    }

    public class TaskRequestDto
    {
        public Guid Id { get; set; }
        public string CommandLine { get; set; } = string.Empty;
        public string? Result { get; set; }
    }
}
