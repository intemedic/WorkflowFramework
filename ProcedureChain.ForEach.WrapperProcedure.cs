using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
    internal sealed partial class ProcedureChain
    {
        private partial class ForEach
        {
			/// <summary>
			/// Wrapper class to wrap a chain of ForEach procedures into a single procedure.
			/// </summary>
			private class WrapperProcedure : Procedure, IProcedureOutput<object>
            {
                public ProcedureChain ProcedureChain { private get; set; }

                private int RunningThreadCount { get; set; }
                private object RunningThreadCountSyncObject { get; } = new object();

                public event ProcedureOutputEventHandler<object> Output;

                public void ProcessEachInput(Procedure predecessor, object product)
                {
                    Debug.Assert(this.ProcedureChain.Nodes.Count > 0 && this.ProcedureChain.Nodes[0] is ForEachProductConsumer);

                    lock (this.RunningThreadCountSyncObject)
                    {
                        ++this.RunningThreadCount;
                    }

                    var procedures = this.ProcedureChain.Initialize(predecessor);

                    var lastProcedure = procedures[procedures.Length - 1];

                    if (lastProcedure is IProcedureOutput<object> lastProcedureOutput)
                    {
                        lastProcedureOutput.Output += (sender, forEachProduct) => this.Output?.Invoke(this, forEachProduct);
                    }

                    lastProcedure.Completed += (sender, e) =>
                    {
                        lock (this.RunningThreadCountSyncObject)
                        {
                            --this.RunningThreadCount;

                            if (this.RunningThreadCount == 0)
                            {
                                this.OnCompleted();
                            }
                        }
                    };

                    procedures[0].InvokeProcessInput(product);
                }
            }
        }
    }
}