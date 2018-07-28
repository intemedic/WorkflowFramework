using System;
using System.Diagnostics;
using System.Threading;

namespace Hillinworks.WorkflowFramework
{
	public abstract class Workflow
	{
		public WorkflowStatus Status { get; private set; } = WorkflowStatus.NotStarted;

		internal CancellationTokenSource CancellationTokenSource { get; private set; }

		internal ProcedureTreeNode ProcedureTree { get; set; }

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

			this.ProcedureTree.Completed += this.ProcedureTree_Completed;

			this.CancellationTokenSource = new CancellationTokenSource();

			this.ProcedureTree.Initialize();

			this.Status = WorkflowStatus.Running;

			this.ProcedureTree.Start();
		}

		private void ProcedureTree_Completed(object sender, EventArgs e)
		{
			Debug.Assert(this.Status == WorkflowStatus.Running);
			this.Status = WorkflowStatus.Ended;
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