namespace Hillinworks.WorkflowFramework
{
	public interface IWorkflowInitiator
	{
		IWorkflowBuilder StartWith<TProcedure>()
			where TProcedure : Procedure, new();

		IWorkflowBuilder StartWith<TProcedure>(TProcedure procedure)
			where TProcedure : Procedure;

		IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new();

		IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>(TProcedure procedure)
			where TProcedure : Procedure, IProcedureOutput<TOutput>;
	}
}