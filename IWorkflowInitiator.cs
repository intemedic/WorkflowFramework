using System;

namespace Hillinworks.WorkflowFramework
{
	public interface IWorkflowInitiator
	{
		IWorkflowBuilder StartWith<TProcedure>()
			where TProcedure : Procedure, new();

		IWorkflowBuilder StartWith<TProcedure>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure;

		IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new();

		IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureOutput<TOutput>;
	}


}
