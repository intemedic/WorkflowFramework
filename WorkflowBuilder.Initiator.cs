using System;

namespace Hillinworks.WorkflowFramework
{
	internal static partial class WorkflowBuilder
	{
		public class Initiator : IWorkflowInitiator
		{
			public ProcedureChain ProcedureChain { get; }

			public Initiator(Workflow workflow)
			{
				this.ProcedureChain = new ProcedureChain(workflow);
			}

			public IWorkflowBuilder StartWith<TProcedure>()
				where TProcedure : Procedure, new()
			{
				return this.StartWith(() => new TProcedure());
			}

			public IWorkflowBuilder StartWith<TProcedure>(Func<TProcedure> procedureFactory)
				where TProcedure : Procedure
			{
				this.ProcedureChain.AddInitiator(procedureFactory);
				return new Builder(this.ProcedureChain);
			}

			public IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
			{
				return this.StartWith<TProcedure, TOutput>(() => new TProcedure());
			}

			public IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>(Func<TProcedure> procedureFactory)
				where TProcedure : Procedure, IProcedureOutput<TOutput>
			{
				this.ProcedureChain.AddInitiator<TProcedure, TOutput>(procedureFactory);
				return new Builder<TOutput>(this.ProcedureChain);
			}
		}

	}
}
