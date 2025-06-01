using System;
using System.Collections;
using System.Collections.Generic;

namespace EilansPlugins
{
    namespace Collections
    {
        public class PriorityList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
        {
            private LinkedList<T> list = new LinkedList<T>();
            private List<LinkedListNode<T>> iterators = new List<LinkedListNode<T>>();
            private Comparison<T> comparison;
            private int isSorted = 0;

            public int Count => ((ICollection<T>)list).Count;
            public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;
            public bool IsSynchronized => ((ICollection)list).IsSynchronized;
            public object SyncRoot => ((ICollection)list).SyncRoot;

            public PriorityList(Comparison<T> comparison) => this.comparison += comparison;

            public T this[int index]
            {
                get
                {
                    if (isSorted <= index) Init();
                    return iterators[index].Value;
                }
                set
                {
                    isSorted = index;
                    RemoveAt(index);
                    Add(value);
                }
            }

            void Init()
            {
                LinkedListNode<T> node;
                int index;

                if (isSorted <= 0)
                {
                    node = list.First;
                    index = 0;
                }
                else
                {
                    node = iterators[isSorted - 1].Next;
                    index = isSorted;
                }

                while (node != null)
                {
                    if (index < iterators.Count) iterators[index] = node;
                    else iterators.Add(node);

                    node = node.Next;
                    index++;
                }

                isSorted = Count;
            }

            void Init(int target)
            {
                LinkedListNode<T> node;
                int index;

                if (isSorted <= 0)
                {
                    node = list.First;
                    index = 0;
                }
                else
                {
                    node = iterators[isSorted - 1].Next;
                    index = isSorted;
                }

                while (node != null && index <= target)
                {
                    if (index < iterators.Count) iterators[index] = node;
                    else iterators.Add(node);

                    node = node.Next;
                    index++;
                }

                isSorted = target + 1;
            }

            public void Add(T item)
            {
                if (Count == 0)
                {
                    list.AddFirst(item);
                    return;
                }

                if ((comparison(item, list.First.Value) < 0))
                {
                    list.AddFirst(item);
                    isSorted = 0;
                    return;
                }

                if ((comparison(list.Last.Value, item) < 0))
                {
                    list.AddLast(item);
                    return;
                }

                int left = 0, right = Count - 1;
                int mid = (left + right) / 2;

                while (left <= right)
                {
                    mid = (left + right) / 2;

                    if (mid >= iterators.Count) Init(mid);
                    if (mid == left) break;

                    //只修改了判断的条件，相当于将大于等于归为一类。
                    if (comparison(this[mid], item) >= 0)
                        right = mid;
                    else
                        left = mid;
                }

                list.AddAfter(iterators[mid], item);
                isSorted = mid;
            }

            public bool Remove(T item)
            {
                if (isSorted < Count) Init();

                int l = 0, r = Count, mid = (l + r) / 2;

                while (l < r)
                {
                    mid = (l + r) / 2;

                    if (r == mid) break;
                    else if (comparison(iterators[mid].Value, item) <= 0) l = mid + 1;
                    else r = mid;
                }

                if (iterators[mid].Value.Equals(item))
                {
                    list.Remove(iterators[mid]);
                    isSorted = mid;
                    return true;
                }
                else return false;
            }

            public bool RemoveAt(int index)
            {
                try
                {
                    if (isSorted <= index) Init();

                    list.Remove(iterators[index]);
                    isSorted = index;

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }

            public void Clear() => ((ICollection<T>)list).Clear();

            public bool Contains(T item) => ((ICollection<T>)list).Contains(item);

            public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)list).CopyTo(array, arrayIndex);

            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)list).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();

            public void CopyTo(Array array, int index) => ((ICollection)list).CopyTo(array, index);
        }
    }
}
