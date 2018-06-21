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
				this.ProcedureChain.AddInitiator<TProcedure>();
				return new Builder(this.ProcedureChain);
			}

			public IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
			{
				this.ProcedureChain.AddInitiator<TProcedure, TOutput>();
				return new Builder<TOutput>(this.ProcedureChain);
			}
		}

	}
}
