using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using slf4net;

namespace Hillinworks.WorkflowFramework
{
    internal class ProcedureTreeNode
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ProcedureTreeNode));


        public ProcedureTreeNode(Procedure procedure, Type inputType, Type outputType)
        {
            this.Procedure = procedure;
            this.InputType = inputType;
            this.OutputType = outputType;

            if (this.Procedure is IProcedureOutput<object> outputInterface)
            {
                outputInterface.Output += this.OnProcedureOutput;
            }

        }

        private List<ProcedureTreeNode> Successors { get; } = new List<ProcedureTreeNode>();

        private List<ProcedureTreeNode> ProductConsumers { get; } = new List<ProcedureTreeNode>();

        private List<Task> ProductConsumerExecutionTasks { get; }
            = new List<Task>();

        private IEnumerable<ProcedureTreeNode> Children => this.Successors.Union(this.ProductConsumers);

        public Procedure Procedure { get; }
        public Type InputType { get; }
        public Type OutputType { get; }

        private bool IsStarted { get; set; }

        private CancellationToken CancellationToken { get; set; }

        internal async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;

            cancellationToken.ThrowIfCancellationRequested();

            this.IsStarted = true;

            // wait until our procedure is executed
            this.Procedure.ExecutionTask = Task.Run(
                () => this.Procedure.InternalExecuteAsync(cancellationToken),
                    cancellationToken);

            try
            {
                await this.Procedure.ExecutionTask;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error when executing procedure {this.Procedure.GetType().Name}: {ex.FormatMessage()}");
                throw;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // execute and wait until all successors are completed
            await Task.WhenAll(
                this.Successors.Select(
                    s => Task.Run(() => s.ExecuteAsync(cancellationToken), cancellationToken)));

            cancellationToken.ThrowIfCancellationRequested();

            // wait until all product consumers are completed
            await Task.WhenAll(this.ProductConsumerExecutionTasks);
        }

        private void OnProcedureOutput(Procedure procedure, object output)
        {
            this.Procedure.IncrementProductCount();
            Parallel.ForEach(this.ProductConsumers,
                consumer =>
                {
                    lock (consumer)
                    {
                        if (!consumer.IsStarted)
                        {
                            consumer.Procedure.InputProcessor = ProcedureInputProcessor.Create(
                                consumer.Procedure,
                                this.CancellationToken);

                            var task = Task.Run(
                                () => consumer.ExecuteAsync(this.CancellationToken),
                                this.CancellationToken);

                            this.ProductConsumerExecutionTasks.Add(task);

                            consumer.IsStarted = true;
                        }

                        consumer.Procedure.InputProcessor.HandleInput(output);
                    }
                });
        }

        public void AddSuccessor(ProcedureTreeNode node)
        {
            node.Procedure.Predecessor = this.Procedure;
            this.Successors.Add(node);
        }

        public void AddProductConsumer(ProcedureTreeNode node)
        {
            if (this.OutputType == null)
            {
                throw new InvalidOperationException("this node does not have an output");
            }

            if (node.InputType == null)
            {
                throw new ArgumentException("specified node does not accept an input", nameof(node));
            }

            if (!node.InputType.IsAssignableFrom(this.OutputType))
            {
                throw new ArgumentException($"specified node does not accept an input of type {this.OutputType.Name}",
                    nameof(node));
            }

            node.Procedure.Predecessor = this.Procedure;
            this.ProductConsumers.Add(node);
        }

        public void Initialize(Workflow workflow)
        {
            this.Procedure.InteralInitialize(workflow);

            foreach (var child in this.Children)
            {
                child.Initialize(workflow);
            }
        }

        public IEnumerable<Procedure> EnumerateProcedures()
        {
            yield return this.Procedure;
            foreach (var child in this.Children)
            {
                foreach (var procedure in child.EnumerateProcedures())
                {
                    yield return procedure;
                }
            }
        }

        public void OnCancelled()
        {
            this.Procedure.OnCancelled();
            foreach (var child in this.Children)
            {
                child.OnCancelled();
            }
        }

        public void OnFaulted()
        {
            this.Procedure.OnFaulted();
            foreach (var child in this.Children)
            {
                child.OnFaulted();
            }
        }
    }
}