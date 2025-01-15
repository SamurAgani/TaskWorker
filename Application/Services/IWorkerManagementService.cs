using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IWorkerManagementService
    {
        WorkerDto AddWorker(string name);
        void RemoveWorker(Guid workerId);
        Guid EnqueueTask(Guid workerId, string commandLine);
        List<WorkerDto> GetAllWorkers();
        WorkerDto? GetWorkerById(Guid id);
        TaskRequestDto? DequeueNextTask(Guid workerId);
        void UpdateTaskResult(Guid workerId, Guid taskId, string result);
    }
}
