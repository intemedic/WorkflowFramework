namespace Hillinworks.WorkflowFramework
{
    public interface IProcedureOutput<out TOutput>: IProcedureOutputProductCount
    {
        event ProcedureOutputEventHandler<TOutput> Output;

    }
}