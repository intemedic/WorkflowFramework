using System.Threading;
using System.Threading.Tasks;
using Tronmedi.Collections;

namespace Hillinworks.WorkflowFramework
{
    internal class SequentialProcedureInputProcessor : IProcedureInputProcessor
    {
        public SequentialProcedureInputProcessor(Procedure procedure, CancellationToken cancellationToken)
        {
            this.Procedure = procedure;
        }

        private Procedure Procedure { get; }
        private FeedQueue<object> ProductQueue { get; } = new FeedQueue<object>();

        private Task ProcessTask { get; set; }

        public Task FinishAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ProductQueue.SignalComplete();
            return this.ProcessTask;
        }

        public void HandleInput(object product)
        {
            this.ProductQueue.Enqueue(product);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.ProcessTask = Task.Run(
                () => this.ProcessQueue(cancellationToken),
                cancellationToken);

            return Task.CompletedTask;
        }

        private async Task ProcessQueue(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var product = this.ProductQueue.Dequeue();
                if (product == null)
                {
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                await this.Procedure.InvokeProcessInputAsync(product, cancellationToken);
            }
        }
    }
}