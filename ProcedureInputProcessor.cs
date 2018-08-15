using System;
using System.Threading;

namespace Hillinworks.WorkflowFramework
{
    internal static class ProcedureInputProcessor
    {
        public static IProcedureInputProcessor Create(Procedure procedure, CancellationToken cancellationToken)
        {
            var strategy = ((IProductConsumerInputConcurrentStrategy)procedure).InputConcurrentStrategy;
            switch (strategy)
            {
                case InputConcurrentStrategy.Sequential:
                    return new SequentialProcedureInputProcessor(procedure, cancellationToken);
                case InputConcurrentStrategy.Parallel:
                    return new ParallelProcedureInputProcessor(procedure, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}