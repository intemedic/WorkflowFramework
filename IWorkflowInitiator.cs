namespace Hillinworks.WorkflowFramework
{
	public interface IWorkflowInitiator
	{
		IWorkflowBuilder StartWith<TProcedure>()
			where TProcedure : Procedure, new();

		IWorkflowBuilder<TOutput> StartWith<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new();
	}


}
