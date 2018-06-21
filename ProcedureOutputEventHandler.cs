namespace Hillinworks.WorkflowFramework
{
	public delegate void ProcedureOutputEventHandler<in TOutput>(Procedure procedure, TOutput output);
}