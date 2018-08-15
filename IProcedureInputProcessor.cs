using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    internal interface IProcedureInputProcessor
    {
        Task FinishAsync(CancellationToken cancellationToken);
        void HandleInput(object product);
        Task StartAsync(CancellationToken cancellationToken);
    }
}