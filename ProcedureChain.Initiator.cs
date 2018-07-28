using System;
using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
        
		private sealed class Initiator : ProcedureNode
		{
			public Initiator(Func<Procedure> procedureFactory) : base(procedureFactory)
			{
			}
			
			protected override void Initialize(Procedure procedure, Procedure predecessor)
			{
				Debug.Assert(predecessor == null);
			}
		}



	}
}
