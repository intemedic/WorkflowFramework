using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using slf4net;

namespace Hillinworks.WorkflowFramework
{
    internal class ParallelProcedureInputProcessor : IProcedureInputProcessor
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ParallelProcedureInputProcessor));
        private Procedure Procedure { get; }
        private CancellationToken CancellationToken { get; }

        private List<object> PendingProducts { get; }
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
                lock (this.PendingProducts)
                {
                    if (!this.IsStarted)
                    {
                        Logger.Debug($"{this.Procedure.DebugName}: Input processor not started yet, add product to pending queue: [{product}] ({product.GetHashCode()})");
                        this.PendingProducts.Add(product);
                        return;
                    }
                }
            }

            // ReSharper disable once InconsistentlySynchronizedField
            Logger.Debug($"{this.Procedure.DebugName}: Processing product: [{product}] ({product.GetHashCode()})");

            var task = Task.Run(
                () => this.Procedure.InvokeProcessInputAsync(product, this.CancellationToken),
                this.CancellationToken);

            this.ProcessTasks.Add(task);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.IsStarted = true;
            lock (this.PendingProducts)
            {
                foreach (var product in this.PendingProducts)
                {
                    Logger.Debug($"{this.Procedure.DebugName}: Processing pending product: [{product}] ({product.GetHashCode()})");
                    this.HandleInput(product);
                }
            }

            return Task.CompletedTask;
        }
    }
}