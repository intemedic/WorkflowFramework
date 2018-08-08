using System;
using System.Diagnostics;
using System.Threading;

namespace Hillinworks.WorkflowFramework
{
    public abstract class Procedure
    {
        private Workflow _workflow;
        public bool IsCompleted { get; private set; }

        protected virtual TimeSpan Timeout { get; } = TimeSpan.FromMinutes(1);
        private Timer TimeoutTimer { get; }

        public Workflow Workflow
        {
            get => _workflow;
            private set
            {
                if (_workflow != null)
                {
                    throw new InvalidOperationException("workflow has been set already");
                }

                _workflow = value;
                this.CancellationToken.Register(this.OnCancelled);
            }
        }

        internal bool IsStarted { get; private set; }

        public Procedure Predecessor { get; internal set; }
        protected internal CancellationToken CancellationToken => this.Workflow.CancellationTokenSource.Token;
        public object Context { get; internal set; }

        public event EventHandler Completed;

        protected Procedure()
        {
            this.TimeoutTimer = new Timer(this.OnTimeout);
        }

        private void OnTimeout(object state)
        {
            throw new TimeoutException("procedure timed out");
        }

        internal void InteralInitialize(Workflow workflow)
        {
            this.Workflow = workflow;
            this.Initialize();
        }
        protected virtual void Initialize()
        {
        }

        internal void InternalStart()
        {
            this.IsStarted = true;
            this.Start();
        }

        protected void ResetTimeout()
        {
            this.TimeoutTimer.Change(this.Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        }

        protected virtual void Start()
        {
            this.ResetTimeout();
        }

        /// <summary>
        ///     Invoke <see cref="IProcedureInput{TInput}.ProcessInput(TInput)" /> on this procedure with reflection, asserting
        ///     this procedure implements <see cref="IProcedureInput{TInput}" />.
        /// </summary>
        internal void InvokeProcessInput(object input)
        {
            var processInputMethod = typeof(IProcedureInput<>).MakeGenericType(input.GetType())
                .GetMethod(nameof(IProcedureInput<object>.ProcessInput));

            Debug.Assert(processInputMethod != null);

            processInputMethod.Invoke(this, new[] { input });
        }

        protected virtual void OnCancelled()
        {
            this.CleanUp();
        }

        protected virtual void OnCompleted()
        {
            Debug.Assert(!this.IsCompleted);
            this.CleanUp();
            this.IsCompleted = true;
            this.Completed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void CleanUp()
        {
            this.TimeoutTimer.Dispose();
        }

        public int GetTotalProductCount()
        {
            if (this is IProcedureOutputProductCount productCountInterface)
            {
                return productCountInterface.TotalProductCount;
            }

            return 0;
        }
    }

}