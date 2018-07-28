using System;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		private sealed class ForEachProductConsumer : ProcedureNode
		{
			public ForEachProductConsumer(Func<Procedure> procedureFactory) : base(procedureFactory)
			{
			}

			protected override void Initialize(Procedure procedure, Procedure predecessor)
			{
				procedure.Start();
			}

		}
	}
}