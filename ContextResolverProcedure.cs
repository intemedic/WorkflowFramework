using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    internal class ContextResolverProcedure<TOutputContext>
        : Procedure
            , IProcedureInput<ProductContext>
            , IProcedureOutput<ProductContext<TOutputContext>>
    {
        public bool Distinct { get; }
        private HashSet<ProductContext<TOutputContext>> ResolvedContexts { get; }

        public ContextResolverProcedure(bool distinct)
        {
            this.Distinct = distinct;
            if (distinct)
            {
                this.ResolvedContexts = new HashSet<ProductContext<TOutputContext>>();
            }
            else
            {
                this.TotalProductCount = this.Predecessor.GetTotalProductCount();
            }
        }

        public Task ProcessInputAsync(ProductContext input, CancellationToken cancellationToken)
        {
            var output = input.Resolve<TOutputContext>();

            if (this.Distinct)
            {
                Debug.Assert(this.ResolvedContexts != null);

                if (this.ResolvedContexts.Contains(output))
                {
                    return Task.CompletedTask;
                }

                this.ResolvedContexts.Add(output);
                this.TotalProductCount = this.ResolvedContexts.Count;
            }

            this.Output?.Invoke(this, output);
            return Task.CompletedTask;
        }

        protected override Task FinishAsync(CancellationToken cancellationToken)
        {
            if (this.Distinct)
            {
                Debug.Assert(this.ResolvedContexts != null);
                this.ResolvedContexts.Clear();
            }

            return base.FinishAsync(cancellationToken);
        }

        public InputConcurrentStrategy InputConcurrentStrategy => InputConcurrentStrategy.Parallel;
        public int TotalProductCount { get; private set; }
        public event ProcedureOutputEventHandler<ProductContext<TOutputContext>> Output;
    }
}