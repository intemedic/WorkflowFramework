namespace Hillinworks.WorkflowFramework
{
    public interface IProcedureInput<in TInput> : IProductConsumerStartTime, IProductConsumerInputConcurrentStrategy
    {
		void ProcessInput(TInput input);
	    
    }
}