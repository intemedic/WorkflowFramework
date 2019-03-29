using System;

namespace Hillinworks.WorkflowFramework
{
    public enum InputConcurrentStrategy
    {
        /// <summary>
        /// One and only one input will be processed. Exceeded input will cause an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        Single,
        /// <summary>
        /// A lock will be used to ensure ProcessInput calls are sequential according
        /// to predecessor's output order.
        /// </summary>
        Sequential,
        /// <summary>
        /// ProcessInput will be called in parallel, disrepect the order of predecessor's
        /// output order. However, for each output of predecessor, the ProcessInput call
        /// and its corresponding Output event is still executed sequentially in the same
        /// thread.
        /// </summary>
        Parallel
    }
}
