using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{

    public interface IProcedureInput<in TInput>
        : IProductConsumerInputConcurrentStrategy
    {
        Task ProcessInputAsync(TInput input, CancellationToken cancellationToken);
    }
}