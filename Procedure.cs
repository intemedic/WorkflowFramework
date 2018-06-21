using System;
using System.Diagnostics;

namespace Hillinworks.WorkflowFramework
{
	public abstract class Procedure
	{
		public event EventHandler Completed;

		public bool IsCompleted { get; private set; }

		public Workflow Workflow { get; private set; }
		public Procedure Predecessor { get; private set; }

		internal virtual void InternalInitialize(Workflow workflow, Procedure predecessor)
		{
			this.Workflow = workflow;
			this.Predecessor = predecessor;
		}

		protected internal virtual void Initialize()
		{

		}

		protected internal virtual void Start()
		{

		}

		/// <summary>
		/// Invoke <see cref="IProcedureInput{TInput}.ProcessInput(TInput)"/> on this procedure with reflection, asserting
		/// this procedure implements <see cref="IProcedureInput{TInput}"/>.
		/// </summary>
		internal void InvokeProcessInput(object input)
		{
			var processInputMethod = typeof(IProcedureInput<>).MakeGenericType(input.GetType())
				.GetMethod(nameof(IProcedureInput<object>.ProcessInput));

			Debug.Assert(processInputMethod != null);

			processInputMethod.Invoke(this, new[] { input });

		}
		
		protected virtual void OnCompleted()
		{
			this.IsCompleted = true;
			this.Completed?.Invoke(this, EventArgs.Empty);
		}

		protected void SubscribeCompleted(Procedure predecessor)
		{
			predecessor.Completed += (sender, e) =>
			  {
				  this.OnPredessorCompleted();
			  };
		}

		protected virtual void OnPredessorCompleted()
		{
		}

	}

}
