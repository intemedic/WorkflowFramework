using System;
using System.Collections;

namespace Hillinworks.WorkflowFramework.Utilities.Threading
{
    public sealed class BinaryPriorityQueue : IPriorityQueue
    {
        private IComparer Comparer { get; }
        private ArrayList InnerList { get; } = new ArrayList();

        public BinaryPriorityQueue()
            : this(System.Collections.Comparer.Default)
        {
        }

        public BinaryPriorityQueue(IComparer comparer)
        {
            this.Comparer = comparer;
        }

        public BinaryPriorityQueue(int capacity)
            : this(System.Collections.Comparer.Default, capacity)
        {
        }

        public BinaryPriorityQueue(IComparer comparer, int capacity)
        {
            this.Comparer = comparer;
            this.InnerList.Capacity = capacity;
        }

        private BinaryPriorityQueue(ArrayList core, IComparer comp, bool copy)
        {
            if (copy)
            {
                this.InnerList = core.Clone() as ArrayList;
            }
            else
            {
                this.InnerList = core;
            }

            this.Comparer = comp;
        }

        /// <summary>
        ///     Push an object onto the PQ
        /// </summary>
        /// <param name="object">The new object</param>
        /// <returns>
        ///     The index in the list where the object is _now_. This will change when objects are taken from or put onto the
        ///     PQ.
        /// </returns>
        public int Push(object @object)
        {
            var p = this.InnerList.Count;
            this.InnerList.Add(@object); // E[p] = O
            do
            {
                if (p == 0)
                {
                    break;
                }

                var p2 = (p - 1) / 2;
                if (this.OnCompare(p, p2) < 0)
                {
                    this.SwitchElements(p, p2);
                    p = p2;
                }
                else
                {
                    break;
                }
            } while (true);

            return p;
        }

        /// <summary>
        ///     Get the smallest object and remove it.
        /// </summary>
        /// <returns>The smallest object</returns>
        public object Pop()
        {
            var result = this.InnerList[0];
            int p = 0, p1, p2, pn;
            this.InnerList[0] = this.InnerList[this.InnerList.Count - 1];
            this.InnerList.RemoveAt(this.InnerList.Count - 1);
            do
            {
                pn = p;
                p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (this.InnerList.Count > p1 && this.OnCompare(p, p1) > 0) // links kleiner
                {
                    p = p1;
                }

                if (this.InnerList.Count > p2 && this.OnCompare(p, p2) > 0) // rechts noch kleiner
                {
                    p = p2;
                }

                if (p == pn)
                {
                    break;
                }

                this.SwitchElements(p, pn);
            } while (true);

            return result;
        }

        /// <summary>
        ///     Notify the PQ that the object at position i has changed
        ///     and the PQ needs to restore order.
        ///     Since you dont have access to any indexes (except by using the
        ///     explicit IList.this) you should not call this function without knowing exactly
        ///     what you do.
        /// </summary>
        /// <param name="i">The index of the changed object.</param>
        public void Update(int i)
        {
            var p = i;
            int p2;
            do // aufsteigen
            {
                if (p == 0)
                {
                    break;
                }

                p2 = (p - 1) / 2;
                if (this.OnCompare(p, p2) < 0)
                {
                    this.SwitchElements(p, p2);
                    p = p2;
                }
                else
                {
                    break;
                }
            } while (true);

            if (p < i)
            {
                return;
            }

            do
            {
                var pn = p;
                var p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (this.InnerList.Count > p1 && this.OnCompare(p, p1) > 0) // links kleiner
                {
                    p = p1;
                }

                if (this.InnerList.Count > p2 && this.OnCompare(p, p2) > 0) // rechts noch kleiner
                {
                    p = p2;
                }

                if (p == pn)
                {
                    break;
                }

                this.SwitchElements(p, pn);
            } while (true);
        }

        /// <summary>
        ///     Get the smallest object without removing it.
        /// </summary>
        /// <returns>The smallest object</returns>
        public object Peek()
        {
            if (this.InnerList.Count > 0)
            {
                return this.InnerList[0];
            }

            return null;
        }

        public bool Contains(object value)
        {
            return this.InnerList.Contains(value);
        }

        public void Clear()
        {
            this.InnerList.Clear();
        }

        public int Count => this.InnerList.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            this.InnerList.CopyTo(array, index);
        }

        public object Clone()
        {
            return new BinaryPriorityQueue(this.InnerList, this.Comparer, true);
        }

        public bool IsSynchronized => this.InnerList.IsSynchronized;

        public object SyncRoot => this;

        bool IList.IsReadOnly => false;

        object IList.this[int index]
        {
            get => this.InnerList[index];
            set
            {
                this.InnerList[index] = value;
                this.Update(index);
            }
        }

        int IList.Add(object o)
        {
            return this.Push(o);
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotSupportedException();
        }

        bool IList.IsFixedSize => false;

        private void SwitchElements(int i, int j)
        {
            var h = this.InnerList[i];
            this.InnerList[i] = this.InnerList[j];
            this.InnerList[j] = h;
        }

        private int OnCompare(int i, int j)
        {
            return this.Comparer.Compare(this.InnerList[i], this.InnerList[j]);
        }

        public static BinaryPriorityQueue Syncronized(BinaryPriorityQueue p)
        {
            return new BinaryPriorityQueue(ArrayList.Synchronized(p.InnerList), p.Comparer, false);
        }

        public static BinaryPriorityQueue ReadOnly(BinaryPriorityQueue p)
        {
            return new BinaryPriorityQueue(ArrayList.ReadOnly(p.InnerList), p.Comparer, false);
        }
    }
}