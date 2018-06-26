using System;

namespace Hillinworks.WorkflowFramework
{
	internal static partial class WorkflowBuilder
	{

		public class BuilderForEach<TBaseBuilder> : IWorkflowBuilderForEach<TBaseBuilder>
		{
			public BuilderForEach(ProcedureChain procedureChain, Func<TBaseBuilder> baseBuilderFactory)
			{
				this.ProcedureChain = procedureChain;
				this.BaseBuilderFactory = baseBuilderFactory;
			}

			public ProcedureChain ProcedureChain { get; }
			public Func<TBaseBuilder> BaseBuilderFactory { get; }

			public IWorkflowBuilderForEach<IWorkflowBuilder> AddSuccessor<TProcedure>() 
				where TProcedure : Procedure, new()
			{
				this.ProcedureChain.AddSuccessor<TProcedure>();
				return new BuilderForEach<IWorkflowBuilder>(
					this.ProcedureChain,
					() => new Builder(this.ProcedureChain));
			}


			public IWorkflowBuilderForEach<IWorkflowBuilder<TOutput>, TOutput> AddSuccessor<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
			{
				this.ProcedureChain.AddSuccessor<TProcedure, TOutput>();

				return new BuilderForEach<IWorkflowBuilder<TOutput>, TOutput>(
					this.ProcedureChain,
					() => new Builder<TOutput>(this.ProcedureChain));
			}

			public TBaseBuilder EndForEach()
			{
				this.ProcedureChain.EndForEach();
				return this.BaseBuilderFactory();
			}
		}

		public class BuilderForEach<TBaseBuilder, TPredecessorProduct>
			: BuilderForEach<TBaseBuilder>, IWorkflowBuilderForEach<TBaseBuilder, TPredecessorProduct>
		{
			public BuilderForEach(ProcedureChain procedureChain, Func<TBaseBuilder> baseBuilderFactory) 
				: base(procedureChain, baseBuilderFactory)
			{
			}
			
			public IWorkflowBuilderForEach<IWorkflowBuilder<TOutput>, TOutput> AddProductConsumer<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new()
			{
				this.ProcedureChain.AddProductConsumer<TProcedure, TPredecessorProduct, TOutput>();

				return new BuilderForEach<IWorkflowBuilder<TOutput>, TOutput>(
					this.ProcedureChain,
					() => new Builder<TOutput>(this.ProcedureChain));
			}

		}


	}
}
