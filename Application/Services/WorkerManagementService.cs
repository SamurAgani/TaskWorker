using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class WorkerManagementService : IWorkerManagementService
    {
        private readonly IWorkerRepository _workerRepository;

        public WorkerManagementService(IWorkerRepository workerRepository)
        {
            _workerRepository = workerRepository;
        }

        public WorkerDto AddWorker(string name)
        {
            var worker = new Worker(name);
            _workerRepository.AddWorker(worker);
            return MapToDto(worker);
        }

        public void RemoveWorker(Guid workerId)
        {
            _workerRepository.RemoveWorker(workerId);
        }

        public Guid EnqueueTask(Guid workerId, string commandLine)
        {
            var worker = _workerRepository.GetWorkerById(workerId);
            if (worker == null)
                throw new Exception("Worker not found.");
            var task = new TaskRequest(commandLine);
            worker.AddTask(task);
            _workerRepository.UpdateWorker(worker);
            return task.Id;
        }

        public List<WorkerDto> GetAllWorkers()
        {
            return _workerRepository
                .GetWorkers()
                .Select(MapToDto)
                .ToList();
        }

        public WorkerDto? GetWorkerById(Guid id)
        {
            var worker = _workerRepository.GetWorkerById(id);
            return worker == null ? null : MapToDto(worker);
        }

        private WorkerDto MapToDto(Worker worker)
        {
            return new WorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                IsActive = worker.IsActive,
                Tasks = worker.Tasks
                    .Select(t => new TaskRequestDto
                    {
                        Id = t.Id,
                        CommandLine = t.CommandLine,
                        Result = t.Result,
                    }).ToList()
            };
        }

        public TaskRequestDto? DequeueNextTask(Guid workerId)
        {
            var worker = _workerRepository.GetWorkerById(workerId);
            if (worker == null) return null;

            var task = worker.DequeueNextTask();
            if (task == null) return null;

            _workerRepository.UpdateWorker(worker);

            return new TaskRequestDto
            {
                Id = task.Id,
                CommandLine = task.CommandLine
            };
        }


        public void UpdateTaskResult(Guid workerId, Guid taskId, string result)
        {
            _workerRepository.UpdateTaskResult(workerId, taskId, result);
        }
    }
}