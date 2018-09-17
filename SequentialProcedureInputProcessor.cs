using System.Threading;
using System.Threading.Tasks;
using slf4net;
using Tronmedi.Collections;

namespace Hillinworks.WorkflowFramework
{
    internal class SequentialProcedureInputProcessor : IProcedureInputProcessor
    {

        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(SequentialProcedureInputProcessor));
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
            Logger.Debug($"{this.Procedure.DebugName}: Enqueuing product: [{product}] ({product.GetHashCode()})");

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
                    Logger.Debug($"{this.Procedure.DebugName}: Input exhausted");
                    return;
                }

                Logger.Debug($"{this.Procedure.DebugName}: Processing product: [{product}] ({product.GetHashCode()})");

                cancellationToken.ThrowIfCancellationRequested();

                await this.Procedure.InvokeProcessInputAsync(product, cancellationToken);
            }
        }
    }
}