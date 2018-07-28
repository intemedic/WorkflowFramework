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
				return this.AddSuccessor(() => new TProcedure());
			}

			public IWorkflowBuilder AddSuccessor<TProcedure>(Func<TProcedure> procedureFactory)
				where TProcedure : Procedure
			{
				this.ProcedureChain.AddSuccessor(procedureFactory);
				return new Builder(this.ProcedureChain);

			}

			public IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
			{
				return this.AddSuccessor<TProcedure, TOutput>(() => new TProcedure());
			}

			public IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>(Func<TProcedure> procedureFactory)
				where TProcedure : Procedure, IProcedureOutput<TOutput>
			{
				this.ProcedureChain.AddSuccessor(procedureFactory);
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
				return this.AddProductConsumer(() => new TProcedure());
			}

			public IWorkflowBuilder AddProductConsumer<TProcedure>(Func<TProcedure> procedureFactory)
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>
			{
				this.ProcedureChain.AddProductConsumer<TProcedure, TPredecessorProduct>(procedureFactory);
				return new Builder(this.ProcedureChain);
			}

			public IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new()
			{
				return this.AddProductConsumer<TProcedure, TOutput>(() => new TProcedure());
			}

			public IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>(Func<TProcedure> procedureFactory) where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>
			{
				this.ProcedureChain.AddProductConsumer<TProcedure, TPredecessorProduct, TOutput>(procedureFactory);
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
