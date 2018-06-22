using System;
using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
    internal sealed partial class ProcedureChain
    {
        [DebuggerDisplay("Parallel: {" + nameof(ProcedureChainLength) + "} procedures")]
        private partial class Parallel : ProcedureNode
        {
            private ProcedureChain ProcedureChain { get; }

#if DEBUG
            private int ProcedureChainLength => this.ProcedureChain.Nodes.Count;
#endif

            public Parallel(ProcedureChain procedureChain) : base(typeof(WrapperProcedure))
            {
                this.ProcedureChain = procedureChain;
            }

            protected override Procedure CreateProcedure()
            {
                var parallelProcedure = (WrapperProcedure)base.CreateProcedure();

                parallelProcedure.ProcedureChain = this.ProcedureChain;

                return parallelProcedure;
            }

            protected override void Initialize(Procedure procedure, Procedure predecessor)
            {
                Debug.Assert(predecessor is IProcedureOutput<object>);
                var outputPredecessor = (IProcedureOutput<object>)predecessor;

                Debug.Assert(procedure is WrapperProcedure);
                var parallelProcedure = (WrapperProcedure)procedure;

                outputPredecessor.Output += (sender, product) =>
                {
                    parallelProcedure.ParallelProcessInput(predecessor, product);
                };

                procedure.Start();
            }
        }
    }
}
