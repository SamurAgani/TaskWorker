using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Worker
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        private Queue<TaskRequest> _tasks = new();
        public List<TaskRequest> Tasks => _tasks.ToList();

        public Worker(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            IsActive = true;
        }

        public void AddTask(TaskRequest task) => _tasks.Enqueue(task);

        public TaskRequest? DequeueNextTask() => _tasks.Count > 0 ? _tasks.Dequeue() : null;

        public void Deactivate() => IsActive = false;

        public void UpdateTaskResult(Guid id, string result)
        {
            var task = _tasks.FirstOrDefault(x => x.Id == id);
            if(task != null) 
                task.Result = result;
        }
    }
}
