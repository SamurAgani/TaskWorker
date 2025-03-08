using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CLIServices
{
    public class WorkerAdminService : IWorkerAdminService
    {
        private readonly IAdminClient _adminClient;
        public WorkerAdminService(IAdminClient adminClient)
        {
            _adminClient = adminClient;
        }
        public async Task<string> AddWorkerAsync(string name)
        {
            var cmd = $"ADD_WORKER|{name}";
            return await _adminClient.SendCommandAsync(cmd);
        }
        public async Task<string> RemoveWorkerAsync(string workerId)
        {
            var cmd = $"REMOVE_WORKER|{workerId}";
            return await _adminClient.SendCommandAsync(cmd);
        }
        public async Task<string> EnqueueTaskAsync(string workerId, string commandLine)
        {
            var cmd = $"ENQUEUE_TASK|{workerId}|{commandLine}";
            return await _adminClient.SendCommandAsync(cmd);
        }
        public async Task<string> GetStatusAsync(string? workerId)
        {
            var cmd = workerId == null ? "STATUS" : $"STATUS|{workerId}";
            return await _adminClient.SendCommandAsync(cmd);
        }
    }
}