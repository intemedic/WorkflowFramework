namespace Hillinworks.WorkflowFramework
{

	public interface IWorkflowBuilderParallel<out TBaseBuilder>
	{
		IWorkflowBuilderParallel<IWorkflowBuilder> AddSuccessor<TProcedure>()
			where TProcedure : Procedure, new();
		IWorkflowBuilderParallel<IWorkflowBuilder<TOutput>, TOutput> AddSuccessor<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new();

		TBaseBuilder EndParallel();
	}

	public interface IWorkflowBuilderParallel<out TBaseBuilder, out TPredecessorProduct> 
		: IWorkflowBuilderParallel<TBaseBuilder>
	{
		IWorkflowBuilderParallel<IWorkflowBuilder<TOutput>, TOutput> AddProductConsumer<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new();
	}


}
