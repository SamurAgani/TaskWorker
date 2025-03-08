using Domain.Entities;
namespace Application.ServerServices
{
    public class InMemoryWorkerRepository : IWorkerRepository
    {
        private readonly Dictionary<Guid, Worker> _workers = new();

        public Worker AddWorker(Worker worker)
        {
            _workers[worker.Id] = worker;
            return worker;
        }

        public void RemoveWorker(Guid workerId) => _workers.Remove(workerId);

        public Worker? GetWorkerById(Guid workerId) =>
            _workers.TryGetValue(workerId, out var worker) ? worker : null;

        public IEnumerable<Worker> GetAllWorkers() => _workers.Values;

        public void UpdateWorker(Worker worker) =>
            _workers[worker.Id] = worker;

        public IEnumerable<Worker> GetWorkers() => _workers.Values;
        public void UpdateTaskResult(Guid workerId, Guid id, string result)
        {
            var worker = _workers[workerId];
            var task = worker.Tasks.FirstOrDefault(x => x.Id == id);
            if (task != null)
                task.Result = result;
        }
    }
}
