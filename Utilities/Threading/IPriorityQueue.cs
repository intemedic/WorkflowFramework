using System;
using System.Collections;

namespace Hillinworks.WorkflowFramework.Utilities.Threading
{
    public interface IPriorityQueue : ICloneable, IList
    {
        int Push(object @object);
        object Pop();
        object Peek();
        void Update(int i);
    }
}