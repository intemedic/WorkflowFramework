namespace Hillinworks.WorkflowFramework
{

	public interface IWorkflowBuilderForEach<out TBaseBuilder>
	{
		IWorkflowBuilderForEach<IWorkflowBuilder> AddSuccessor<TProcedure>()
			where TProcedure : Procedure, new();
		IWorkflowBuilderForEach<IWorkflowBuilder<TOutput>, TOutput> AddSuccessor<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new();

		TBaseBuilder EndForEach();
	}

	public interface IWorkflowBuilderForEach<out TBaseBuilder, out TPredecessorProduct> 
		: IWorkflowBuilderForEach<TBaseBuilder>
	{
		IWorkflowBuilderForEach<IWorkflowBuilder<TOutput>, TOutput> AddProductConsumer<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new();
	}


}
