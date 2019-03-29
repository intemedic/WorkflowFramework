using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using slf4net;

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
        private BlockingCollection<object> ProductQueue { get; } = new BlockingCollection<object>();

        private Task ProcessTask { get; set; }

        public Task FinishAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ProductQueue.CompleteAdding();
            return this.ProcessTask;
        }

        public void HandleInput(object product)
        {
            Logger.Debug($"{this.Procedure.DebugName}: Enqueuing product: [{product}] ({product.GetHashCode()})");

            this.ProductQueue.Add(product);
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
            foreach (var product in this.ProductQueue.GetConsumingEnumerable())
            {
                cancellationToken.ThrowIfCancellationRequested();

                Logger.Debug($"{this.Procedure.DebugName}: Processing product: [{product}] ({product.GetHashCode()})");

                cancellationToken.ThrowIfCancellationRequested();

                await this.Procedure.InvokeProcessInputAsync(product, cancellationToken);
            }

            Logger.Debug($"{this.Procedure.DebugName}: Input exhausted");
        }
    }
}