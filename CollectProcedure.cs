﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    internal class CollectProcedure<TInput, TOutput>
        : Procedure
            , IProcedureInput<TInput>
            , IProcedureOutput<TOutput>
    {
        public CollectProcedure(Func<IEnumerable<TInput>, IEnumerable<TOutput>> transform)
        {
            this.Transform = transform;
        }

        private Func<IEnumerable<TInput>, IEnumerable<TOutput>> Transform { get; }

        private List<TInput> Inputs { get; }
            = new List<TInput>();

        public Task ProcessInputAsync(TInput input, CancellationToken cancellationToken)
        {
            this.Inputs.Add(input);
            return Task.CompletedTask;
        }

        public InputConcurrentStrategy InputConcurrentStrategy => InputConcurrentStrategy.Sequential;
        public int TotalProductCount => this.Predecessor.GetTotalProductCount();
        public event ProcedureOutputEventHandler<TOutput> Output;


        protected override Task FinishAsync(CancellationToken cancellationToken)
        {
            if (this.Output != null)
            {
                foreach (var input in this.Transform(this.Inputs))
                {
                    this.Output.Invoke(this, input);
                }
            }

            return base.FinishAsync(cancellationToken);
        }
    }
}