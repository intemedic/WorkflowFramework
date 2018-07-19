using System;
using System.Collections.Generic;

namespace Hillinworks.WorkflowFramework
{
	internal sealed partial class ProcedureChain
	{
		public ProcedureChain(Workflow workflow, ProcedureChain baseChain = null)
		{
			this.Workflow = workflow;
			this.BaseChain = baseChain;
		}

		private List<ProcedureNode> Nodes { get; }
			= new List<ProcedureNode>();

		public bool IsClosed { get; private set; }

		public Workflow Workflow { get; }
		private ProcedureChain BaseChain { get; }
		private bool IsSubChain => this.BaseChain != null;
		private Type CurrentProductType { get; set; }

		public void AddProductConsumer<TProcedure, TInput, TOutput>()
			where TProcedure : Procedure, IProcedureInput<TInput>, IProcedureOutput<TOutput>, new()
		{
			this.AddProductConsumer<TProcedure, TInput>();

			this.CurrentProductType = typeof(TOutput);
		}

		internal void AddProductConsumer<TProcedure, TInput>() 
			where TProcedure : Procedure, IProcedureInput<TInput>, new()
		{
			this.CheckClosed();
			this.CheckRequireInitator();
			this.CheckInputType<TInput>();

			if (this.IsSubChain && this.Nodes.Count == 0)
			{
				this.Nodes.Add(new SubworkflowInitiator(typeof(TProcedure)));
			}
			else
			{
				this.Nodes.Add(new ProductConsumer(typeof(TProcedure)));
			}
		}

		private void CheckInputType<TInput>()
		{
			if (this.CurrentProductType == null || !typeof(TInput).IsAssignableFrom(this.CurrentProductType))
			{
				throw new ArgumentException(
					$"current product type '{this.CurrentProductType?.Name}' cannot be consumed by specified procedure");
			}
		}

		public void AddSuccessor<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
		{
			this.CheckClosed();
			this.CheckRequireInitator();

			this.Nodes.Add(new Successor(typeof(TProcedure)));
			this.CurrentProductType = typeof(TOutput);
		}

		public void AddSuccessor<TProcedure>()
			where TProcedure : Procedure, new()
		{
			this.CheckClosed();
			this.CheckRequireInitator();

			this.Nodes.Add(new Successor(typeof(TProcedure)));
			this.CurrentProductType = null;
		}

		public void AddInitiator<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
		{
			this.CheckClosed();
			this.CheckCanAddInitator();

			this.Nodes.Add(new Initiator(typeof(TProcedure)));
			this.CurrentProductType = typeof(TOutput);
		}

		private void CheckCanAddInitator()
		{
			if (this.IsSubChain || this.Nodes.Count != 0)
			{
				throw new InvalidOperationException("cannot add an initiator when the chain is not empty");
			}
		}

		private void CheckRequireInitator()
		{
			if (!this.IsSubChain && this.Nodes.Count == 0)
			{
				throw new InvalidOperationException("an initiator is required");
			}
		}

		public void AddInitiator<TProcedure>() 
			where TProcedure : Procedure, new()
		{
			this.CheckClosed();
			this.CheckCanAddInitator();

			this.Nodes.Add(new Initiator(typeof(TProcedure)));
			this.CurrentProductType = null;
		}

		/// <summary>
		///     Instantialize and initialize a chain of procedures.
		/// </summary>
		/// <param name="predecessor">
		///     The predecessor procedure of this chain if it is a subworkflow procedure chain, otherwise it
		///     should be null.
		/// </param>
		/// <returns>The instantialized procedures.</returns>
		public Procedure[] Initialize(Procedure predecessor = null)
		{
			if (!this.IsClosed)
			{
				throw new InvalidOperationException("cannot start an open procedure chain, close it first");
			}

			var procedures = new List<Procedure>();

			// ReSharper disable once LoopCanBeConvertedToQuery (for readability)
			foreach (var node in this.Nodes)
			{
				var procedure = node.Initialize(this.Workflow, predecessor);
				procedures.Add(procedure);
				predecessor = procedure;
			}

			return procedures.ToArray();
		}

		public void Close()
		{
			this.CheckClosed();

			if (this.IsSubChain)
			{
				throw new InvalidOperationException("a sub-workflow is not closed");
			}

			if (this.Nodes.Count == 0)
			{
				throw new InvalidOperationException("this workflow does not contain any procedure");
			}

			this.IsClosed = true;
		}

		private void CheckClosed()
		{
			if (this.IsClosed)
			{
				throw new InvalidOperationException("cannot modify a closed procedure chain");
			}
		}

		internal ProcedureChain CreateSubchain()
		{
			return new ProcedureChain(this.Workflow, this)
			{
				CurrentProductType = this.CurrentProductType
			};
		}

		public void AddSubworkflow(ProcedureChain subchain)
		{
			if (subchain.Nodes.Count == 0)
			{
				throw new InvalidOperationException("the sub-workflow procedure chain does not contain any procedure");
			}

			subchain.BaseChain.CurrentProductType = subchain.CurrentProductType;
			subchain.IsClosed = true;

			this.Nodes.Add(new Subworkflow(subchain));

		}
	}
}