namespace Hillinworks.WorkflowFramework
{
	public interface IProcedureInput<in TInput>
	{
		void ProcessInput(TInput input);
	}
}