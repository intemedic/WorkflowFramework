using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    internal class CollectProcedure<TInput>
        : Procedure
            , IProcedureInput<TInput>
            , IProcedureOutput<TInput>
    {
        private List<TInput> Inputs { get; }
            = new List<TInput>();

        public Task ProcessInputAsync(TInput input, CancellationToken cancellationToken)
        {
            this.Inputs.Add(input);
            return Task.CompletedTask;
        }

        protected override Task FinishAsync(CancellationToken cancellationToken)
        {
            if (this.Output != null)
            {
                foreach (var input in this.Inputs)
                {
                    this.Output.Invoke(this, input);
                }
            }

            return base.FinishAsync(cancellationToken);
        }

        public InputConcurrentStrategy InputConcurrentStrategy => InputConcurrentStrategy.Sequential;
        public int TotalProductCount => this.Predecessor.GetTotalProductCount();
        public event ProcedureOutputEventHandler<TInput> Output;
    }
}