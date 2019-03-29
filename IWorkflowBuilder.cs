using System;
using System.Collections.Generic;

namespace Hillinworks.WorkflowFramework
{
    public interface IWorkflowBuilder
    {
        /// <summary>
        /// Set the context of following procedures.
        /// </summary>
        IWorkflowBuilder SetContext(object context);

        /// <summary>
        /// Add a successor which will execute when its predecessor is finished.
        /// </summary>
        IWorkflowBuilder AddSuccessor<TProcedure>()
            where TProcedure : Procedure, new();

        /// <summary>
        /// Add a successor which will execute when its predecessor is finished.
        /// </summary>
        IWorkflowBuilder AddSuccessor<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure;

        /// <summary>
        /// Add a successor which will execute when its predecessor is finished. The successor
        /// can yield output.
        /// </summary>
        IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
            where TProcedure : Procedure, IProcedureOutput<TOutput>, new();

        /// <summary>
        /// Add a successor which will execute when its predecessor is finished. The successor
        /// can yield output.
        /// </summary>
        IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureOutput<TOutput>;

    }

    public interface IWorkflowBuilder<out TPredecessorProduct> : IWorkflowBuilder
    {
        /// <summary>
        /// Set the context of following procedures.
        /// </summary>
        new IWorkflowBuilder<TPredecessorProduct> SetContext(object context);

        /// <summary>
        /// Add a successor which will execute when its predecessor is finished. The predecssor's
        /// output will be bypassed and sent to the next procedure.
        /// </summary>
        IWorkflowBuilder<TPredecessorProduct> AddBypassSuccessor<TProcedure>()
            where TProcedure : Procedure, new();

        /// <summary>
        /// Add a successor which will execute when its predecessor is finished. The predecssor's
        /// output will be bypassed and sent to the next procedure.
        /// </summary>
        IWorkflowBuilder<TPredecessorProduct> AddBypassSuccessor<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure;

        /// <summary>
        /// Add a procedure which consumes its predecessor's output.
        /// </summary>
        IWorkflowBuilder AddProductConsumer<TProcedure>()
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new();

        /// <summary>
        /// Add a procedure which consumes its predecessor's output.
        /// </summary>
        IWorkflowBuilder AddProductConsumer<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>;

        /// <summary>
        /// Add a procedure which consumes its predecessor's output. The predecessor's output then
        /// will be sent to the next procedure.
        /// </summary>
        IWorkflowBuilder<TPredecessorProduct> AddBypassProductConsumer<TProcedure>()
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new();

        /// <summary>
        /// Add a procedure which consumes its predecessor's output. The predecessor's output then
        /// will be sent to the next procedure.
        /// </summary>
        IWorkflowBuilder<TPredecessorProduct> AddBypassProductConsumer<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>;

        /// <summary>
        /// Add a procedure which consumes its predecessor's output. The consumer then yields its own
        /// output.
        /// </summary>
        IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new();

        /// <summary>
        /// Add a procedure which consumes its predecessor's output. The consumer then yields its own
        /// output.
        /// </summary>
        IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>;

        /// <summary>
        /// Collect the outputs of the last procedure and redispatch them with a transform after the last procedure is finished.
        /// </summary>
        IWorkflowBuilder<TOutput> Collect<TOutput>(
            Func<IEnumerable<TPredecessorProduct>, IEnumerable<TOutput>> transform );
        
        /// <summary>
        /// Dispatch the outputs of the last procedure by an interval.
        /// </summary>
        IWorkflowBuilder<TPredecessorProduct> DelayDispatch(TimeSpan duration);
    }
}