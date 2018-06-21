using System;

namespace Hillinworks.WorkflowFramework
{
	internal static partial class WorkflowBuilder
	{

		public class BuilderParallel<TBaseBuilder> : IWorkflowBuilderParallel<TBaseBuilder>
		{
			public BuilderParallel(ProcedureChain procedureChain, Func<TBaseBuilder> baseBuilderFactory)
			{
				this.ProcedureChain = procedureChain;
				this.BaseBuilderFactory = baseBuilderFactory;
			}

			public ProcedureChain ProcedureChain { get; }
			public Func<TBaseBuilder> BaseBuilderFactory { get; }

			public IWorkflowBuilderParallel<IWorkflowBuilder> AddSuccessor<TProcedure>() 
				where TProcedure : Procedure, new()
			{
				this.ProcedureChain.AddSuccessor<TProcedure>();
				return new BuilderParallel<IWorkflowBuilder>(
					this.ProcedureChain,
					() => new Builder(this.ProcedureChain));
			}


			public IWorkflowBuilderParallel<IWorkflowBuilder<TOutput>, TOutput> AddSuccessor<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
			{
				this.ProcedureChain.AddSuccessor<TProcedure, TOutput>();

				return new BuilderParallel<IWorkflowBuilder<TOutput>, TOutput>(
					this.ProcedureChain,
					() => new Builder<TOutput>(this.ProcedureChain));
			}

			public TBaseBuilder EndParallel()
			{
				this.ProcedureChain.EndParallel();
				return this.BaseBuilderFactory();
			}
		}

		public class BuilderParallel<TBaseBuilder, TPredecessorProduct>
			: BuilderParallel<TBaseBuilder>, IWorkflowBuilderParallel<TBaseBuilder, TPredecessorProduct>
		{
			public BuilderParallel(ProcedureChain procedureChain, Func<TBaseBuilder> baseBuilderFactory) 
				: base(procedureChain, baseBuilderFactory)
			{
			}
			
			public IWorkflowBuilderParallel<IWorkflowBuilder<TOutput>, TOutput> AddProductConsumer<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new()
			{
				this.ProcedureChain.AddProductConsumer<TProcedure, TPredecessorProduct, TOutput>();

				return new BuilderParallel<IWorkflowBuilder<TOutput>, TOutput>(
					this.ProcedureChain,
					() => new Builder<TOutput>(this.ProcedureChain));
			}

		}


	}
}
