using System;
using System.Threading;
using System.Threading.Tasks;
using slf4net;

namespace Hillinworks.WorkflowFramework
{
    internal class SingleProcedureInputProcessor : IProcedureInputProcessor
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(SingleProcedureInputProcessor));

        private Procedure Procedure { get; }
        private CancellationToken CancellationToken { get; }

        private bool IsStarted { get; set; }
        private object PendingProduct { get; set; }
        private bool HasProduct { get; set; }
        private Task ProcessTask { get; set; }

        public SingleProcedureInputProcessor(Procedure procedure, CancellationToken cancellationToken)
        {
            this.Procedure = procedure;
            this.CancellationToken = cancellationToken;
        }

        public Task FinishAsync(CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (!this.HasProduct)
                {
                    throw new InvalidOperationException($"No product was input for {this.Procedure.DebugName}");
                }

                return this.ProcessTask;
            }
        }

        public void HandleInput(object product)
        {
            lock (this)
            {
                if (this.HasProduct)
                {
                    throw new InvalidOperationException($"{this.Procedure.DebugName} accepts only one single product");
                }

                this.HasProduct = true;

                if (!this.IsStarted)
                {
                    Logger.Debug($"{this.Procedure.DebugName}: Input processor not started yet, product pended: [{product}] ({product.GetHashCode()})");
                    this.PendingProduct = product;
                }
                else
                {
                    this.ProcessTask = this.Procedure.InvokeProcessInputAsync(product, this.CancellationToken);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            lock (this)
            {
                this.IsStarted = true;

                if (this.HasProduct)
                {
                    this.ProcessTask = this.Procedure.InvokeProcessInputAsync(this.PendingProduct, cancellationToken);
                    this.PendingProduct = null;
                }

                return Task.CompletedTask;
            }
        }
    }
}