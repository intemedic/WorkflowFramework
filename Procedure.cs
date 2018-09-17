using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using slf4net;

namespace Hillinworks.WorkflowFramework
{
    public abstract class Procedure
    {
        private Workflow _workflow;
        public bool IsCompleted { get; private set; }

        protected virtual TimeSpan Timeout { get; } = TimeSpan.FromMinutes(1);
        private Timer TimeoutTimer { get; }
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(Procedure));

#if DEBUG
        internal string DebugName => this.GetType().Name;
#endif

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
        
        public Procedure Predecessor { get; internal set; }
        protected internal CancellationToken CancellationToken => this.Workflow.CancellationTokenSource.Token;
        public object Context { get; internal set; }
        public Task ExecutionTask { get; internal set; }

        internal IProcedureInputProcessor InputProcessor { get; set; }

        public event EventHandler Completed;

        protected Procedure()
        {
            this.TimeoutTimer = new Timer(this.OnTimeout);
        }

        private void OnTimeout(object state)
        {
#if DEBUG
            //Debug.Assert(false, $"procedure {this.DebugName} timed out");
#else
            throw new TimeoutException("procedure timed out");
#endif
        }

        internal void InteralInitialize(Workflow workflow)
        {
            Logger.Debug($"Initializing procedure {this.DebugName}");
            this.Workflow = workflow;
            this.Initialize();
        }

        protected virtual void Initialize()
        {
        }

        internal async Task InternalExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            Logger.Debug($"Executing procedure {this.DebugName}");

            await this.ExecuteAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (this.InputProcessor != null)
            {
                Logger.Debug($"Starting input processor of procedure {this.DebugName}");

                // start input processor only after ExecuteAsync is done, to
                // ensure initialization works are done
                await this.InputProcessor.StartAsync(cancellationToken);
            }

            if (this.Predecessor != null)
            {
                // wait until predecessor is finished
                await this.Predecessor.ExecutionTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (this.InputProcessor != null)
            {
                Logger.Debug($"Finishing input processor of procedure {this.DebugName}");

                // wait until all input handling tasks are finished
                await this.InputProcessor.FinishAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            Logger.Debug($"Finishing procedure {this.DebugName}");

            await this.FinishAsync(cancellationToken);

            this.OnCompleted();
        }

        protected void ResetTimeout()
        {
            this.TimeoutTimer.Change(this.Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        }

        protected virtual Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.ResetTimeout();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Invoke <see cref="IProcedureInput{TInput}.ProcessInputAsync" /> on this procedure with reflection, asserting
        ///     this procedure implements <see cref="IProcedureInput{TInput}" />.
        /// </summary>
        internal Task InvokeProcessInputAsync(object input, CancellationToken cancellationToken)
        {
            var processInputMethod = typeof(IProcedureInput<>)
                .MakeGenericType(input.GetType())
                .GetMethod(nameof(IProcedureInput<object>.ProcessInputAsync));

            Debug.Assert(processInputMethod != null);

            return (Task)processInputMethod.Invoke(this, new[] { input, cancellationToken });
        }

        protected internal virtual void OnCancelled()
        {
            this.CleanUp();
        }

        protected internal virtual void OnFaulted()
        {
            this.CleanUp();
        }

        protected internal virtual void OnCompleted()
        {
            if (this.IsCompleted)
            {
                throw new InvalidOperationException("this procedure is already completed");
            }

            if (_productCount != this.GetTotalProductCount())
            {
                var message = "this procedure has not yielded enough products "
                              + $"({_productCount} yielded, {this.GetTotalProductCount()} expected";

#if DEBUG
                Debug.Assert(false, message);
#else
                throw new InvalidOperationException(message);
#endif
            }

            Logger.Debug($"Procedure {this.DebugName} has completed successfully");

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

        private int _productCount;

        internal void IncrementProductCount()
        {
            Interlocked.Increment(ref _productCount);
        }

        protected internal virtual Task FinishAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}