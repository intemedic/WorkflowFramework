using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    public abstract class Workflow
    {
        internal ProcedureTreeNode ProcedureTree { get; set; }

        protected virtual void OnCompleted()
        {
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var builder = new WorkflowBuilder.Initiator(this);
            this.Build(builder);

            if (this.ProcedureTree == null)
            {
                throw new InvalidOperationException("cannot start an empty workflow");
            }

            this.Initialize();
            this.ProcedureTree.Initialize(this);

            try
            {
                await this.ProcedureTree.ExecuteAsync(cancellationToken);
                this.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                this.ProcedureTree.OnCancelled();
                throw;
            }
            catch (Exception)
            {
                this.ProcedureTree.OnFaulted();
                throw;
            }
            finally
            {
                this.CleanUp();
            }
        }

        protected virtual void Initialize()
        {
        }

        protected abstract void Build(IWorkflowInitiator initiator);

        protected IEnumerable<Procedure> EnumerateProcedures()
        {
            return this.ProcedureTree.EnumerateProcedures();
        }

        protected virtual void CleanUp()
        {

        }
    }
}