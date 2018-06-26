using System;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		private sealed class ForEachProductConsumer : ProcedureNode
		{
			public ForEachProductConsumer(Type procedureType) : base(procedureType)
			{
			}

			protected override void Initialize(Procedure procedure, Procedure predecessor)
			{
				procedure.Start();
			}

		}
	}
}