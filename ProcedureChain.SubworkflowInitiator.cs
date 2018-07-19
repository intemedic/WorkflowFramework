using System;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		private sealed class SubworkflowInitiator : ProcedureNode
		{
			public SubworkflowInitiator(Type procedureType) : base(procedureType)
			{
			}

			protected override void Initialize(Procedure procedure, Procedure predecessor)
			{
				procedure.Start();
			}

		}
	}
}