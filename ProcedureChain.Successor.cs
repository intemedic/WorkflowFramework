using System;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		private sealed class Successor : ProcedureNode
		{
			public Successor(Func<Procedure> procedureFactory) : base(procedureFactory)
			{
			}

			protected override void Initialize(Procedure procedure, Procedure predecessor)
			{
				predecessor.Completed += (sender, e) =>
				  {
					  procedure.Start();
				  };
			}
		}


	}
}
