namespace Hillinworks.WorkflowFramework
{
	internal static partial class WorkflowBuilder
	{
		public class Builder : IWorkflowBuilder
		{
			public Builder(ProcedureTreeNode node)
			{
				this.Node = node;
			}

			public ProcedureTreeNode Node { get; }

			public IWorkflowBuilder SetContext(object context)
			{
				this.Node.Context = context;
				return this;
			}

			public IWorkflowBuilder AddSuccessor<TProcedure>()
				where TProcedure : Procedure, new()
			{
				return this.AddSuccessor(new TProcedure());
			}

			public IWorkflowBuilder AddSuccessor<TProcedure>(TProcedure procedure)
				where TProcedure : Procedure
			{
				var successorNode = new ProcedureTreeNode(procedure, null, null);
				this.Node.AddSuccessor(successorNode);
				return new Builder(successorNode);
			}

			public IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
			{
				return this.AddSuccessor<TProcedure, TOutput>(new TProcedure());
			}

			public IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>(TProcedure procedure)
				where TProcedure : Procedure, IProcedureOutput<TOutput>
			{
				var successorNode = new ProcedureTreeNode(procedure, null, typeof(TOutput));
				this.Node.AddSuccessor(successorNode);
				return new Builder<TOutput>(successorNode);
			}
		}


		public class Builder<TPredecessorProduct> : Builder, IWorkflowBuilder<TPredecessorProduct>
		{
			public Builder(ProcedureTreeNode node)
				: base(node)
			{
			}

			public new IWorkflowBuilder<TPredecessorProduct> SetContext(object context)
			{
				return (IWorkflowBuilder<TPredecessorProduct>) base.SetContext(context);
			}

			public IWorkflowBuilder AddProductConsumer<TProcedure>()
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new()
			{
				return this.AddProductConsumer(new TProcedure());
			}

			public IWorkflowBuilder AddProductConsumer<TProcedure>(TProcedure procedure)
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>
			{
				var productConsumeNode = new ProcedureTreeNode(procedure, typeof(TPredecessorProduct), null);
				this.Node.AddProductConsumer(productConsumeNode);
				return new Builder(productConsumeNode);
			}

			public IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new()
			{
				return this.AddProductConsumer<TProcedure, TOutput>(new TProcedure());
			}

			public IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>(TProcedure procedure)
				where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>
			{
				var productConsumeNode = new ProcedureTreeNode(procedure, typeof(TPredecessorProduct), typeof(TOutput));
				this.Node.AddProductConsumer(productConsumeNode);
				return new Builder<TOutput>(productConsumeNode);
			}
		}
	}
}