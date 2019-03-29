using System;
using System.Collections.Generic;

namespace Hillinworks.WorkflowFramework
{
    internal static partial class WorkflowBuilder
    {
        public abstract class BuilderBase
        {
            protected object Context { get; set; }
        }

        public class Builder : BuilderBase, IWorkflowBuilder
        {
            public Builder(ProcedureTreeNode node, object context)
            {
                this.Node = node;
                this.Context = context;
            }

            public ProcedureTreeNode Node { get; }

            public IWorkflowBuilder SetContext(object context)
            {
                this.Context = context;
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
                procedure.Context = this.Context;
                var successorNode = new ProcedureTreeNode(procedure, null, null);
                this.Node.AddSuccessor(successorNode);
                return new Builder(successorNode, this.Context);
            }

            public IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
                where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
            {
                return this.AddSuccessor<TProcedure, TOutput>(new TProcedure());
            }

            public IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>(TProcedure procedure)
                where TProcedure : Procedure, IProcedureOutput<TOutput>
            {
                procedure.Context = this.Context;
                var successorNode = new ProcedureTreeNode(procedure, null, typeof(TOutput));
                this.Node.AddSuccessor(successorNode);
                return new Builder<TOutput>(successorNode, this.Context);
            }

        }


        public class Builder<TPredecessorProduct> : Builder, IWorkflowBuilder<TPredecessorProduct>
        {
            public Builder(ProcedureTreeNode node, object context)
                : base(node, context)
            {
            }

            public new IWorkflowBuilder<TPredecessorProduct> SetContext(object context)
            {
                return (IWorkflowBuilder<TPredecessorProduct>)base.SetContext(context);
            }

            public IWorkflowBuilder<TPredecessorProduct> AddBypassSuccessor<TProcedure>()
                where TProcedure : Procedure, new()
            {
                return this.AddBypassSuccessor(new TProcedure());
            }

            public IWorkflowBuilder<TPredecessorProduct> AddBypassSuccessor<TProcedure>(TProcedure procedure)
                where TProcedure : Procedure
            {
                procedure.Context = this.Context;

                return this.AddProductConsumer<BypassProcedure<TPredecessorProduct>, TPredecessorProduct>(
                    new BypassProcedure<TPredecessorProduct>(procedure));
            }

            public IWorkflowBuilder AddProductConsumer<TProcedure>()
                where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new()
            {
                return this.AddProductConsumer(new TProcedure());
            }

            public IWorkflowBuilder AddProductConsumer<TProcedure>(TProcedure procedure)
                where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>
            {
                procedure.Context = this.Context;
                var productConsumeNode = new ProcedureTreeNode(procedure, typeof(TPredecessorProduct), null);
                this.Node.AddProductConsumer(productConsumeNode);
                return new Builder(productConsumeNode, this.Context);
            }

            public IWorkflowBuilder<TPredecessorProduct> AddBypassProductConsumer<TProcedure>()
                where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new()
            {
                return this.AddBypassProductConsumer(new TProcedure());
            }

            public IWorkflowBuilder<TPredecessorProduct> AddBypassProductConsumer<TProcedure>(TProcedure procedure)
                where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>
            {
                procedure.Context = this.Context;

                return this.AddProductConsumer<BypassProcedure<TProcedure, TPredecessorProduct>, TPredecessorProduct>(
                    new BypassProcedure<TProcedure, TPredecessorProduct>(procedure));
            }

            public IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
                where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new()
            {
                return this.AddProductConsumer<TProcedure, TOutput>(new TProcedure());
            }

            public IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>(TProcedure procedure)
                where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>
            {
                procedure.Context = this.Context;
                var productConsumeNode = new ProcedureTreeNode(procedure, typeof(TPredecessorProduct), typeof(TOutput));
                this.Node.AddProductConsumer(productConsumeNode);
                return new Builder<TOutput>(productConsumeNode, this.Context);
            }

            public IWorkflowBuilder<TOutput> Collect<TOutput>(Func<IEnumerable<TPredecessorProduct>, IEnumerable<TOutput>> transform = null)
            {
                return this.AddProductConsumer<CollectProcedure<TPredecessorProduct, TOutput>, TOutput>(
                    new CollectProcedure<TPredecessorProduct, TOutput>(transform));
            }

            public IWorkflowBuilder<TPredecessorProduct> DelayDispatch(TimeSpan duration)
            {
                return this.AddProductConsumer<DelayDispatchProcedure<TPredecessorProduct>, TPredecessorProduct>(
                    new DelayDispatchProcedure<TPredecessorProduct>(duration));
            }
        }
    }
}