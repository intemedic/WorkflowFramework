using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    internal class ParallelProcedureInputProcessor : IProcedureInputProcessor
    {
        private Procedure Procedure { get; }
        private CancellationToken CancellationToken { get; }

        private List<object> PendingInputs { get; }
            = new List<object>();

        private ConcurrentBag<Task> ProcessTasks { get; }
            = new ConcurrentBag<Task>();

        private bool IsStarted { get; set; }

        public ParallelProcedureInputProcessor(Procedure procedure, CancellationToken cancellationToken)
        {
            this.Procedure = procedure;
            this.CancellationToken = cancellationToken;
        }

        public Task FinishAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.WhenAll(this.ProcessTasks);
        }

        public void HandleInput(object product)
        {
            if (!this.IsStarted)
            {
                lock (this.PendingInputs)
                {
                    if (!this.IsStarted)
                    {
                        this.PendingInputs.Add(product);
                        return;
                    }
                }
            }

            var task = Task.Run(
                () => this.Procedure.InvokeProcessInputAsync(product, this.CancellationToken),
                this.CancellationToken);

            this.ProcessTasks.Add(task);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.IsStarted = true;
            lock (this.PendingInputs)
            {
                foreach (var input in this.PendingInputs)
                {
                    this.HandleInput(input);
                }
            }

            return Task.CompletedTask;
        }
    }
}