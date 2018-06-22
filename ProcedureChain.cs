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
			this.CurrentProcedureChain = this;
		}

		private List<ProcedureNode> Nodes { get; }
			= new List<ProcedureNode>();

		public bool IsClosed { get; private set; }

		public Workflow Workflow { get; }
		private ProcedureChain BaseChain { get; }
		private bool IsParallelChain => this.BaseChain != null;
		private ProcedureChain CurrentProcedureChain { get; set; }
		private Type CurrentProductType { get; set; }

		public void AddProductConsumer<TProcedure, TInput, TOutput>()
			where TProcedure : Procedure, IProcedureInput<TInput>, IProcedureOutput<TOutput>, new()
		{
			this.AddProductConsumer<TProcedure, TInput>();

			this.CurrentProcedureChain.CurrentProductType = typeof(TOutput);
		}

		internal void AddProductConsumer<TProcedure, TInput>() 
			where TProcedure : Procedure, IProcedureInput<TInput>, new()
		{
			this.CheckClosed();
			this.CheckRequireInitator();
			this.CheckInputType<TInput>();

			if (this.CurrentProcedureChain.IsParallelChain && this.CurrentProcedureChain.Nodes.Count == 0)
			{
				this.CurrentProcedureChain.Nodes.Add(new ParallelProductConsumer(typeof(TProcedure)));
			}
			else
			{
				this.CurrentProcedureChain.Nodes.Add(new ProductConsumer(typeof(TProcedure)));
			}
		}

		private void CheckInputType<TInput>()
		{
			if (this.CurrentProcedureChain.CurrentProductType == null || !typeof(TInput).IsAssignableFrom(this.CurrentProcedureChain.CurrentProductType))
			{
				throw new ArgumentException(
					$"current product type '{this.CurrentProcedureChain.CurrentProductType?.Name}' cannot be consumed by specified procedure");
			}
		}

		public void AddSuccessor<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
		{
			this.CheckClosed();
			this.CheckRequireInitator();

			this.CurrentProcedureChain.Nodes.Add(new Successor(typeof(TProcedure)));
			this.CurrentProcedureChain.CurrentProductType = typeof(TOutput);
		}

		public void AddSuccessor<TProcedure>()
			where TProcedure : Procedure, new()
		{
			this.CheckClosed();
			this.CheckRequireInitator();

			this.CurrentProcedureChain.Nodes.Add(new Successor(typeof(TProcedure)));
			this.CurrentProcedureChain.CurrentProductType = null;
		}

		public void AddInitiator<TProcedure, TOutput>()
			where TProcedure : Procedure, IProcedureOutput<TOutput>, new()
		{
			this.CheckClosed();
			this.CheckCanAddInitator();

			this.CurrentProcedureChain.Nodes.Add(new Initiator(typeof(TProcedure)));
			this.CurrentProcedureChain.CurrentProductType = typeof(TOutput);
		}

		private void CheckCanAddInitator()
		{
			if (this.CurrentProcedureChain.IsParallelChain || this.CurrentProcedureChain.Nodes.Count != 0)
			{
				throw new InvalidOperationException("cannot add an initiator when the chain is not empty");
			}
		}

		private void CheckRequireInitator()
		{
			if (!this.CurrentProcedureChain.IsParallelChain && this.CurrentProcedureChain.Nodes.Count == 0)
			{
				throw new InvalidOperationException("an initiator is required");
			}
		}

		public void AddInitiator<TProcedure>() 
			where TProcedure : Procedure, new()
		{
			this.CheckClosed();
			this.CheckCanAddInitator();

			this.CurrentProcedureChain.Nodes.Add(new Initiator(typeof(TProcedure)));
			this.CurrentProcedureChain.CurrentProductType = null;
		}

		public void BeginParallel()
		{
			this.CheckClosed();

			var parallelChain = new ProcedureChain(this.Workflow, this.CurrentProcedureChain)
			{
				CurrentProductType = this.CurrentProcedureChain.CurrentProductType
			};

			this.CurrentProcedureChain.Nodes.Add(new Parallel(parallelChain));

			this.CurrentProcedureChain = parallelChain;
		}

		public void EndParallel()
		{
			this.CheckClosed();

			if (!this.CurrentProcedureChain.IsParallelChain)
			{
				throw new InvalidOperationException("not in a parallel procedure chain");
			}

			if (this.CurrentProcedureChain.Nodes.Count == 0)
			{
				throw new InvalidOperationException("the parallel procedure chain does not contain any procedure");
			}

			this.CurrentProcedureChain.BaseChain.CurrentProductType = this.CurrentProcedureChain.CurrentProductType;
			this.CurrentProcedureChain = this.CurrentProcedureChain.BaseChain;
		}

		/// <summary>
		///     Instantialize and initialize a chain of procedures.
		/// </summary>
		/// <param name="predecessor">
		///     The predecessor procedure of this chain if it is a parallel procedure chain, otherwise it
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

			if (this.CurrentProcedureChain.IsParallelChain)
			{
				throw new InvalidOperationException("a parallel sub-workflow is not closed");
			}

			if (this.CurrentProcedureChain.Nodes.Count == 0)
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
	}
}