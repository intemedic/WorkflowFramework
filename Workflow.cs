using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hillinworks.WorkflowFramework
{
	public abstract class Workflow
	{
		public WorkflowStatus Status { get; private set; } = WorkflowStatus.NotStarted;

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

			procedures[procedures.Length - 1].Completed += (sender, e) =>
			{
				Debug.Assert(this.Status == WorkflowStatus.Running);
				this.Status = WorkflowStatus.Ended;
			};

			this.Status = WorkflowStatus.Running;
			
			procedures[0].Start();
		}

		protected abstract void Build(IWorkflowInitiator builder);
	}
}
