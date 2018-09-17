using System;
using System.Collections.Generic;
using System.Threading;

namespace Hillinworks.WorkflowFramework
{
    public abstract class Workflow
    {
        public WorkflowStatus Status { get; private set; }
            = WorkflowStatus.NotStarted;

        internal CancellationTokenSource CancellationTokenSource { get; private set; }

        internal ProcedureTreeNode ProcedureTree { get; set; }

        protected virtual void OnCompleted()
        {
        }

        public void Start()
        {
            if (this.Status != WorkflowStatus.NotStarted)
            {
                throw new InvalidOperationException(
                    $"cannot start a workflow which is not in {nameof(WorkflowStatus.NotStarted)} state");
            }

            var builder = new WorkflowBuilder.Initiator(this);
            this.Build(builder);

            if (this.ProcedureTree == null)
            {
                throw new InvalidOperationException("cannot start an empty workflow");
            }

            this.CancellationTokenSource = new CancellationTokenSource();

            this.Initialize();
            this.ProcedureTree.Initialize(this);

            this.Status = WorkflowStatus.Running;

            try
            {
                this.ProcedureTree.ExecuteAsync(this.CancellationTokenSource.Token).Wait();
                this.Status = WorkflowStatus.Completed;
                this.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                this.Status = WorkflowStatus.Cancelled;
                this.ProcedureTree.OnCancelled();
            }
            catch (Exception)
            {
                this.Status = WorkflowStatus.Faulted;
                this.ProcedureTree.OnFaulted();
            }
            finally
            {
                this.CleanUp();
            }
        }

        protected virtual void Initialize()
        {
        }

        public void Cancel(bool throwOnFirstException = true)
        {
            if (this.Status != WorkflowStatus.Running)
            {
                throw new InvalidOperationException(
                    $"cannot cancel a workflow which is not in {nameof(WorkflowStatus.Running)} state");
            }

            this.CancellationTokenSource.Cancel(throwOnFirstException);
        }

        protected abstract void Build(IWorkflowInitiator builder);

        protected IEnumerable<Procedure> EnumerateProcedures()
        {
            return this.ProcedureTree.EnumerateProcedures();
        }

        protected virtual void CleanUp()
        {

        }
    }
}