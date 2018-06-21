using System;
using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		private sealed class ProductConsumer : ProcedureNode
		{
			public ProductConsumer(Type procedureType) : base(procedureType)
			{
			}

			protected override void Initialize(Procedure procedure, Procedure predecessor)
			{
				Debug.Assert(predecessor is IProcedureOutput<object>);
				var predecessorOutput = (IProcedureOutput<object>)predecessor;

				predecessorOutput.Output += (sender, product) => procedure.InvokeProcessInput(product);

				procedure.Start();
			}
		}
	}
}