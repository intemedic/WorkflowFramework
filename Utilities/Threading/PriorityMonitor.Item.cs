using System;

namespace Hillinworks.WorkflowFramework.Utilities.Threading
{
    public partial class PriorityMonitor
    {
        private class Item : IComparable
        {
            public Item(int priority, object tag)
            {
                this.Priority = priority;
                this.Tag = tag;
            }

            private int Priority { get; }

            public object Tag { get; }

            public int CompareTo(object other)
            {
                return ((Item) other).Priority - this.Priority;
            }
        }
    }
}