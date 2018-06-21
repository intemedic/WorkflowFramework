using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		private partial class Parallel
		{
			/// <summary>
			/// Wrapper class to wrap a chain of parallel procedures into a single procedure.
			/// </summary>
			private class WrapperProcedure : Procedure, IProcedureOutput<object>
			{
				public ProcedureChain ProcedureChain { private get; set; }

				private int RunningThreadCount { get; set; }
				private object RunningThreadCountSyncObject { get; } = new object();

				public event ProcedureOutputEventHandler<object> Output;

				public void ParallelProcessInput(Procedure predecessor, object product)
				{
					Debug.Assert(this.ProcedureChain.Nodes.Count > 0 && this.ProcedureChain.Nodes[0] is ParallelProductConsumer);

					lock (this.RunningThreadCountSyncObject)
					{
						++this.RunningThreadCount;
					}

					var procedures = this.ProcedureChain.Initialize(predecessor);

					var lastProcedure = procedures[procedures.Length - 1];

					var lastProcedureOutput = lastProcedure as IProcedureOutput<object>;

					if (lastProcedureOutput != null)
					{
						lastProcedureOutput.Output += (sender, parallelProduct) => { this.Output?.Invoke(this, parallelProduct); };
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