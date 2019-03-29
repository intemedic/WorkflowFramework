using System;
using System.Collections.Generic;

namespace Hillinworks.WorkflowFramework
{
    public static class WorkflowBuilderExtensions
    {
        public static IWorkflowBuilder<ProductContext<TOutputContext>> ResolveContext<TOutputContext>(
            this IWorkflowBuilder<ProductContext> builder,
            bool distinct = false)
        {
            return builder.AddProductConsumer<
                ContextResolverProcedure<TOutputContext>,
                ProductContext<TOutputContext>>(
                new ContextResolverProcedure<TOutputContext>(distinct));
        }


        /// <summary>
        /// Collect the outputs of the last procedure and redispatch them after the last procedure is finished.
        /// </summary>
        public static IWorkflowBuilder<TOutput> Collect<TOutput>(
            this IWorkflowBuilder<TOutput> builder)
        {
            return builder.Collect(o => o);
        }
    }
}