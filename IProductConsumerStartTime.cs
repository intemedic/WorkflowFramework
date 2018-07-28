namespace Hillinworks.WorkflowFramework
{
	public interface IProductConsumerStartTime
	{
		ProcedureStartTime StartTime { get; }
	}

	public interface IProcedureInput<in TInput> : IProductConsumerStartTime
	{
		void ProcessInput(TInput input);
	}
}