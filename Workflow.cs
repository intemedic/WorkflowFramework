using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
    public abstract class Workflow
    {
        public WorkflowStatus Status { get; private set; } = WorkflowStatus.NotStarted;

        internal CancellationTokenSource CancellationTokenSource { get; private set; }

        public void Start()
        {
            if (this.Status != WorkflowStatus.NotStarted)
            {
                throw new InvalidOperationException(
                    $"cannot start a workflow which is not in {nameof(WorkflowStatus.NotStarted)} state");
            }

            var builder = new WorkflowBuilder.Initiator(this);
            this.Build(builder);

            builder.ProcedureChain.Close();

            var procedures = builder.ProcedureChain.Initialize();

            Debug.Assert(procedures.Length > 0);

            this.CancellationTokenSource = new CancellationTokenSource();

            procedures[procedures.Length - 1].Completed += (sender, e) =>
            {
                Debug.Assert(this.Status == WorkflowStatus.Running);
                this.Status = WorkflowStatus.Ended;
            };

            this.Status = WorkflowStatus.Running;

            procedures[0].Start();
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
    }
}
