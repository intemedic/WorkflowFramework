using System;
using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		[DebuggerDisplay("Procedure: {" + nameof(ProcedureType) + "}")]
		private abstract class ProcedureNode
		{
			private Func<Procedure> ProcedureFactory { get; }

			protected ProcedureNode(Func<Procedure> procedureFactory)
			{
				this.ProcedureFactory = procedureFactory;
			}

#if DEBUG
			private Type ProcedureType => this.ProcedureFactory.Method.ReturnType;
#endif

			protected virtual Procedure CreateProcedure()
			{
				return this.ProcedureFactory();
			}

			/// <summary>
			/// Instantialize and initialize a procedure, hooking it up with its <paramref name="predecessor"/>'s productive events.
			/// </summary>
			/// <param name="workflow">The workflow which owns the procedure.</param>
			/// <param name="predecessor">The predecessor of the procedure.</param>
			/// <returns>The instantialized procedure.</returns>
			public Procedure Initialize(Workflow workflow, Procedure predecessor)
			{
				var procedure = this.CreateProcedure();
				procedure.InternalInitialize(workflow, predecessor);
				procedure.Initialize();
				this.Initialize(procedure, predecessor);
				return procedure;
			}

			/// <summary>
			/// Initialize a procedure, hooking it up with its <paramref name="predecessor"/>'s productive events.
			/// </summary>
			/// <param name="procedure">The procedure to initialize.</param>
			/// <param name="predecessor">The predecessor to chain up.</param>
			protected abstract void Initialize(Procedure procedure, Procedure predecessor);
		}



	}
}
