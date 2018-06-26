using System;
using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
    internal sealed partial class ProcedureChain
    {
        [DebuggerDisplay("ForEach: {" + nameof(ProcedureChainLength) + "} procedures")]
        private partial class ForEach : ProcedureNode
        {
            private ProcedureChain ProcedureChain { get; }

#if DEBUG
            private int ProcedureChainLength => this.ProcedureChain.Nodes.Count;
#endif

            public ForEach(ProcedureChain procedureChain) : base(typeof(WrapperProcedure))
            {
                this.ProcedureChain = procedureChain;
            }

            protected override Procedure CreateProcedure()
            {
                var forEachProcedure = (WrapperProcedure)base.CreateProcedure();

                forEachProcedure.ProcedureChain = this.ProcedureChain;

                return forEachProcedure;
            }

            protected override void Initialize(Procedure procedure, Procedure predecessor)
            {
                Debug.Assert(predecessor is IProcedureOutput<object>);
                var outputPredecessor = (IProcedureOutput<object>)predecessor;

                Debug.Assert(procedure is WrapperProcedure);
                var forEachProcedure = (WrapperProcedure)procedure;

                outputPredecessor.Output += (sender, product) =>
                {
                    forEachProcedure.ProcessEachInput(predecessor, product);
                };

                procedure.Start();
            }
        }
    }
}
