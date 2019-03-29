using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    internal class BypassProcedure<TInnerProcedure, TInput>
        : Procedure
            , IProcedureInput<TInput>
            , IProcedureOutput<TInput>

        where TInnerProcedure : Procedure, IProcedureInput<TInput>
    {

        public TInnerProcedure InnerProcedure { get; }

        public BypassProcedure(TInnerProcedure innerProcedure)
        {
            this.InnerProcedure = innerProcedure;
        }

        public async Task ProcessInputAsync(TInput input, CancellationToken cancellationToken)
        {
            await this.InnerProcedure.ProcessInputAsync(input, cancellationToken);

            this.Output?.Invoke(this, input);
        }

        public InputConcurrentStrategy InputConcurrentStrategy => this.InnerProcedure.InputConcurrentStrategy;
        public int TotalProductCount => this.InnerProcedure.GetTotalProductCount();
        public event ProcedureOutputEventHandler<TInput> Output;
    }

    internal class BypassProcedure<TInput>
        : Procedure
            , IProcedureInput<TInput>
            , IProcedureOutput<TInput>
    {

        public Procedure InnerProcedure { get; }

        private List<TInput> Inputs { get; }
            = new List<TInput>();

        public BypassProcedure(Procedure innerProcedure)
        {
            this.InnerProcedure = innerProcedure;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.InnerProcedure.ExecutionTask = Task.Run(
                () => this.InnerProcedure.InternalExecuteAsync(cancellationToken),
                cancellationToken);

            return base.ExecuteAsync(cancellationToken);
        }

        protected override async Task FinishAsync(CancellationToken cancellationToken)
        {
            await this.InnerProcedure.ExecutionTask;

            foreach (var input in this.Inputs)
            {
                this.Output?.Invoke(this, input);
            }

            this.Inputs.Clear();
        }

        public Task ProcessInputAsync(TInput input, CancellationToken cancellationToken)
        {
            this.Inputs.Add(input);
            return Task.CompletedTask;
        }

        public InputConcurrentStrategy InputConcurrentStrategy => InputConcurrentStrategy.Sequential;
        public int TotalProductCount => this.Predecessor.GetTotalProductCount();
        public event ProcedureOutputEventHandler<TInput> Output;
    }
}