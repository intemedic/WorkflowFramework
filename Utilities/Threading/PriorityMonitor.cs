using System;
using System.Collections.Generic;
using System.Threading;

namespace Hillinworks.WorkflowFramework.Utilities.Threading
{
    public partial class PriorityMonitor
    {
        private const int InvalidThreadId = -1;

        private bool IsThreadScheduled { get; set; }
        private int ThreadHoldingLock { get; set; } = InvalidThreadId;
        private Dictionary<int, int> ThreadLockCountDictionary { get; } = new Dictionary<int, int>();

        private IPriorityQueue PriorityQueue { get; } = new BinaryPriorityQueue();
        private object QueueLock { get; } = new object();

        private bool IsLockFree { get; set; } = true;

        public void Lock(int priority)
        {
            object waitObject = null;

            var waitForLock = true;
            lock (this.QueueLock)
            {
                if (this.IsThreadScheduled == false && this.IsLockFree || this.CurrentThreadHoldsLock())
                {
                    this.SetCurrentThreadAsLockOwner();
                    waitForLock = false;
                }
                else
                {
                    waitObject = new object();
                    var item = new Item(priority, waitObject);
                    this.PriorityQueue.Push(item);
                }
            }

            if (waitForLock)
            {
                lock (waitObject)
                {
                    Monitor.Wait(waitObject);
                    lock (this.QueueLock)
                    {
                        this.IsThreadScheduled = false;
                        this.SetCurrentThreadAsLockOwner();
                    }
                }
            }
        }

        public void Unlock()
        {
            Item nextItem = null;
            lock (this.QueueLock)
            {
                if (!this.IsLockHeldByCallingThread())
                {
                    throw new InvalidOperationException("Cannot call Unlock as current thread has not called Lock.");
                }

                this.SetLockFree();

                if (this.IsLockFree && this.PriorityQueue.Count != 0)
                {
                    nextItem = (Item) this.PriorityQueue.Pop();
                    this.IsThreadScheduled = true;
                }
            }

            if (nextItem != null)
            {
                var waitObject = nextItem.Tag;
                lock (waitObject)
                {
                    Monitor.Pulse(waitObject);
                }
            }
        }

        private bool IsLockHeldByCallingThread()
        {
            return Thread.CurrentThread.ManagedThreadId == this.ThreadHoldingLock;
        }

        private bool CurrentThreadHoldsLock()
        {
            return this.ThreadHoldingLock == Thread.CurrentThread.ManagedThreadId;
        }

        private void SetCurrentThreadAsLockOwner()
        {
            this.ThreadHoldingLock = Thread.CurrentThread.ManagedThreadId;

            this.IncrementThreadLockCount();
            this.IsLockFree = false;
        }

        private void SetLockFree()
        {
            var decrementedLockCount = this.DecrementLockCount();
            if (decrementedLockCount == 0)
            {
                this.ThreadHoldingLock = InvalidThreadId;
                this.IsLockFree = true;
            }
        }

        private int IncrementThreadLockCount()
        {
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            this.ThreadLockCountDictionary.TryGetValue(currentThreadId, out var lockCount);

            ++lockCount;
            this.ThreadLockCountDictionary[currentThreadId] = lockCount;
            return lockCount;
        }

        private int DecrementLockCount()
        {
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            this.ThreadLockCountDictionary.TryGetValue(currentThreadId, out var lockCount);

            lockCount--;

            if (lockCount == 0)
            {
                this.ThreadLockCountDictionary.Remove(currentThreadId);
            }
            else if (lockCount > 0)
            {
                this.ThreadLockCountDictionary[currentThreadId] = lockCount;
            }
            else
            {
                throw new InvalidOperationException("Cannot call Unlock without corresponding call to Lock");
            }

            return lockCount;
        }
    }
}