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

		IWorkflowBuilder AddSuccessor<TProcedure>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure;


		IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new();

		IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureOutput<TOutput>;
	}

	public interface IWorkflowBuilder<out TPredecessorProduct> : IWorkflowBuilder
	{
		IWorkflowBuilder AddProductConsumer<TProcedure>()
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new();

		IWorkflowBuilder AddProductConsumer<TProcedure>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>;

		IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new();

		IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>;
		
		IWorkflowBuilder<TProduct> AddSubworkflow<TProduct>(
			Func<IWorkflowBuilder<TPredecessorProduct>, IWorkflowBuilder<TProduct>> build);
	}


}
