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
		private bool IsSubChain => this.BaseChain != null;
		private ProcedureChain CurrentProcedureChain { get; set; }
		private Type CurrentProductType { get; set; }

		public void AddProductConsumer<TProcedure, TInput, TOutput>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureInput<TInput>, IProcedureOutput<TOutput>
		{
			this.AddProductConsumer<TProcedure, TInput>(procedureFactory);

			this.CurrentProcedureChain.CurrentProductType = typeof(TOutput);
		}

		internal void AddProductConsumer<TProcedure, TInput>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureInput<TInput>
		{
			this.CheckClosed();
			this.CheckRequireInitator();
			this.CheckInputType<TInput>();

			if (this.CurrentProcedureChain.IsSubChain && this.CurrentProcedureChain.Nodes.Count == 0)
			{
				this.CurrentProcedureChain.Nodes.Add(new ForEachProductConsumer(procedureFactory));
			}
			else
			{
				this.CurrentProcedureChain.Nodes.Add(new ProductConsumer(procedureFactory));
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

		public void AddSuccessor<TProcedure>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure
		{
			this.CheckClosed();
			this.CheckRequireInitator();

			this.CurrentProcedureChain.Nodes.Add(new Successor(procedureFactory));
			this.CurrentProcedureChain.CurrentProductType = null;
		}

		public void AddSuccessor<TProcedure, TOutput>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureOutput<TOutput>
		{
			this.CheckClosed();
			this.CheckRequireInitator();

			this.CurrentProcedureChain.Nodes.Add(new Successor(procedureFactory));
			this.CurrentProcedureChain.CurrentProductType = typeof(TOutput);
		}

		public void AddInitiator<TProcedure, TOutput>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure, IProcedureOutput<TOutput>
		{
			this.CheckClosed();
			this.CheckCanAddInitator();

			this.CurrentProcedureChain.Nodes.Add(new Initiator(procedureFactory));
			this.CurrentProcedureChain.CurrentProductType = typeof(TOutput);
		}

		private void CheckCanAddInitator()
		{
			if (this.CurrentProcedureChain.IsSubChain || this.CurrentProcedureChain.Nodes.Count != 0)
			{
				throw new InvalidOperationException("cannot add an initiator when the chain is not empty");
			}
		}

		private void CheckRequireInitator()
		{
			if (!this.CurrentProcedureChain.IsSubChain && this.CurrentProcedureChain.Nodes.Count == 0)
			{
				throw new InvalidOperationException("an initiator is required");
			}
		}

		public void AddInitiator<TProcedure>(Func<TProcedure> procedureFactory)
			where TProcedure : Procedure
		{
			this.CheckClosed();
			this.CheckCanAddInitator();

			this.CurrentProcedureChain.Nodes.Add(new Initiator(procedureFactory));
			this.CurrentProcedureChain.CurrentProductType = null;
		}

		public void BeginForEach()
		{
			this.CheckClosed();

			var forEachChain = new ProcedureChain(this.Workflow, this.CurrentProcedureChain)
			{
				CurrentProductType = this.CurrentProcedureChain.CurrentProductType
			};

			this.CurrentProcedureChain.Nodes.Add(new ForEach(forEachChain));

			this.CurrentProcedureChain = forEachChain;
		}

		public void EndForEach()
		{
			this.CheckClosed();

			if (!this.CurrentProcedureChain.IsSubChain)
			{
				throw new InvalidOperationException("not in a ForEach procedure chain");
			}

			if (this.CurrentProcedureChain.Nodes.Count == 0)
			{
				throw new InvalidOperationException("the ForEach procedure chain does not contain any procedure");
			}

			this.CurrentProcedureChain.BaseChain.CurrentProductType = this.CurrentProcedureChain.CurrentProductType;
			this.CurrentProcedureChain.IsClosed = true;
			this.CurrentProcedureChain = this.CurrentProcedureChain.BaseChain;
		}

		/// <summary>
		///     Instantialize and initialize a chain of procedures.
		/// </summary>
		/// <param name="predecessor">
		///     The predecessor procedure of this chain if it is a ForEach procedure chain, otherwise it
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

			if (this.CurrentProcedureChain.IsSubChain)
			{
				throw new InvalidOperationException("a ForEach sub-workflow is not closed");
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

		internal ProcedureChain CreateSubchain()
		{
			return new ProcedureChain(this.Workflow, this.CurrentProcedureChain)
			{
				CurrentProductType = this.CurrentProcedureChain.CurrentProductType
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

			this.CurrentProcedureChain.Nodes.Add(new ForEach(subchain));

		}
	}
}