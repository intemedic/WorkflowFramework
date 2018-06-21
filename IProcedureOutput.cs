using System;

namespace Hillinworks.WorkflowFramework
{
	public interface IProcedureOutput<out TOutput>
	{
		event ProcedureOutputEventHandler<TOutput> Output;
	}
}