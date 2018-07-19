using System;

namespace Hillinworks.WorkflowFramework
{
	internal static partial class WorkflowBuilder
	{
		public class Builder : IWorkflowBuilder
		{
			public Builder(ProcedureChain procedureChain)
			{
				this.ProcedureChain = procedureChain;
			}

			public ProcedureChain ProcedureChain { get; }

			public IWorkflowBuilder AddSuccessor<TProcedure>()
				where TProcedure : Procedure, new()
			{
				this.ProcedureChain.AddSuccessor<TProcedure>();
				return new Builder(this.ProcedureChain);
			}

			public IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
			{
				this.ProcedureChain.AddSuccessor<TProcedure, TOutput>();
				return new Builder<TOutput>(this.ProcedureChain);
			}
		}

	}

	internal partial class WorkflowBuilder
	{
		public class Builder<TPredecessorProduct> : Builder, IWorkflowBuilder<TPredecessorProduct>
		{
			public Builder(ProcedureChain procedureChain)
				: base(procedureChain)
			{
			}

			public IWorkflowBuilder AddProductConsumer<TProcedure>() 
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new()
			{
				this.ProcedureChain.AddProductConsumer<TProcedure, TPredecessorProduct>();
				return new Builder(this.ProcedureChain);
			}

			public IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new()
			{
				this.ProcedureChain.AddProductConsumer<TProcedure, TPredecessorProduct, TOutput>();
				return new Builder<TOutput>(this.ProcedureChain);
			}

			public IWorkflowBuilder<TProduct> AddSubworkflow<TProduct>(Func<IWorkflowBuilder<TPredecessorProduct>, IWorkflowBuilder<TProduct>> build)
			{

				var subchain = this.ProcedureChain.CreateSubchain();
				var subchainBuilder = new Builder<TPredecessorProduct>(subchain);
				build(subchainBuilder);
				this.ProcedureChain.AddSubworkflow(subchain);

				return new Builder<TProduct>(this.ProcedureChain);
			}

		}

	}
}
