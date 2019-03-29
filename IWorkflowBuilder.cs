﻿using System;

namespace Hillinworks.WorkflowFramework
{
    public interface IWorkflowBuilder
    {
        IWorkflowBuilder SetContext(object context);

        IWorkflowBuilder AddSuccessor<TProcedure>()
            where TProcedure : Procedure, new();

        IWorkflowBuilder AddSuccessor<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure;

        IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>()
            where TProcedure : Procedure, IProcedureOutput<TOutput>, new();

        IWorkflowBuilder<TOutput> AddSuccessor<TProcedure, TOutput>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureOutput<TOutput>;

    }

    public interface IWorkflowBuilder<out TPredecessorProduct> : IWorkflowBuilder
    {
        new IWorkflowBuilder<TPredecessorProduct> SetContext(object context);

        IWorkflowBuilder<TPredecessorProduct> AddBypassSuccessor<TProcedure>()
            where TProcedure : Procedure, new();

        IWorkflowBuilder<TPredecessorProduct> AddBypassSuccessor<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure;

        IWorkflowBuilder AddProductConsumer<TProcedure>()
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new();

        IWorkflowBuilder AddProductConsumer<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>;

        IWorkflowBuilder<TPredecessorProduct> AddBypassProductConsumer<TProcedure>()
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, new();

        IWorkflowBuilder<TPredecessorProduct> AddBypassProductConsumer<TProcedure>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>;

        IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>()
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>, new();

        IWorkflowBuilder<TOutput> AddProductConsumer<TProcedure, TOutput>(TProcedure procedure)
            where TProcedure : Procedure, IProcedureInput<TPredecessorProduct>, IProcedureOutput<TOutput>;

        IWorkflowBuilder<TPredecessorProduct> Collect();
    }
}