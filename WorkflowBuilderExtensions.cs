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
    }
}