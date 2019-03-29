using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    internal class DelayDispatchProcedure<TInput>
        : Procedure
            , IProcedureInput<TInput>
            , IProcedureOutput<TInput>
    {
        public DelayDispatchProcedure(TimeSpan duration)
        {
            this.Duration = duration;
        }

        public TimeSpan Duration { get; }

        private List<TInput> Inputs { get; }
            = new List<TInput>();

        public Task ProcessInputAsync(TInput input, CancellationToken cancellationToken)
        {
            this.Inputs.Add(input);
            return Task.CompletedTask;
        }

        public InputConcurrentStrategy InputConcurrentStrategy => InputConcurrentStrategy.Sequential;
        public int TotalProductCount => this.Predecessor.GetTotalProductCount();
        public event ProcedureOutputEventHandler<TInput> Output;

        protected override async Task FinishAsync(CancellationToken cancellationToken)
        {
            if (this.Output != null)
            {
                foreach (var input in this.Inputs)
                {
                    this.Output.Invoke(this, input);
                    await Task.Delay(this.Duration, cancellationToken);
                }
            }
        }
    }
}