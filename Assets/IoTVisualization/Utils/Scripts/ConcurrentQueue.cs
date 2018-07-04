using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// A simple blocking queue to work with concurrency. This queue works internally with a queue.
    /// </summary>
    public class ConcurrentQueue<T> : IEnumerable<T>, IEnumerable, ICollection, ICollection<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();

        /// <summary>
        /// Number of elements inside the queue.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        /// Enqueues an item to the queue.
        /// </summary>
        /// <param name="item">To be added item.</param>
        public void Enqueue(T item)
        {
            lock (this)
            {
                _queue.Enqueue(item);
            }
        }

        /// <summary>
        /// Removes and returns the first item in queue.
        /// </summary>
        /// <returns>First item</returns>
        public T Dequeue()
        {
            lock (this)
            {
                return _queue.Dequeue();
            }
        }

        /// <summary>
        /// Returns the first item in queue.
        /// </summary>
        /// <returns>First item</returns>
        public T Peek()
        {
            lock (this)
            {
                return _queue.Peek();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this)
            {
                return _queue.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this)
            {
                return _queue.GetEnumerator();
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (this)
            {
                _queue.CopyTo(array.Cast<T>().ToArray(), index);
            }
        }

        public void Add(T item)
        {
            lock (this)
            {
                _queue.Enqueue(item);
            }
        }

        public void Clear()
        {
            lock (this)
            {
                _queue.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (this)
            {
                return _queue.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this)
            {
                _queue.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        int ICollection<T>.Count
        {
            get
            {
                lock (this)
                {
                    return _queue.Count;
                }
            }
        }

        public bool IsReadOnly { get { return false; } }

        int ICollection.Count
        {
            get
            {
                lock (this)
                {
                    return _queue.Count;
                }
            }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot { get { return null; } }
    }
}
