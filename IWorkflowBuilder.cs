using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{

	public interface IWorkflowBuilder
	{
		IWorkflowBuilder AddSuccessor<TProcedure>()
			where TProcedure : Procedure, new();
		IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new();
	}

	public interface IWorkflowBuilder<out TPredecessorProduct> : IWorkflowBuilder
	{
		IWorkflowBuilder AddProductConsumer<TProcedure>()
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new();

		IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new();

		IWorkflowBuilderForEach<IWorkflowBuilder<TPredecessorProduct>, TPredecessorProduct> BeginForEach();

	}


}
