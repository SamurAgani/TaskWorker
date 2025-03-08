namespace Application.CLIServices
{
    public interface IWorkerAdminService
    {
        Task<string> AddWorkerAsync(string name);
        Task<string> RemoveWorkerAsync(string workerId);
        Task<string> EnqueueTaskAsync(string workerId, string commandLine);
        Task<string> GetStatusAsync(string? workerId);
    }
}
