using System;
using System.Collections.Generic;

namespace EilansPlugin
{
    namespace Container
    {
        public class PriorityList<T> : LinkedList<T>
        {
            Comparison<T> comparison;
            List<LinkedListNode<T>> iterators = new List<LinkedListNode<T>>();
            public PriorityList(Comparison<T> comparison) 
            {
                this.comparison += comparison;
            }
            public void Add(T item)
            {
                if (Count == 0) 
                {
                    base.AddFirst(item);
                    return;
                }
                if ((comparison(item,First.Value) < 0))
                {
                    AddFirst(item);
                    isSorted = 0;
                    return;
                }
                if ((comparison(Last.Value, item) < 0))
                {
                    AddLast(item);
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
                    if (comparison(this[mid],item)>=0)
                        right = mid;
                    else
                        left = mid;
                }
                AddAfter(iterators[mid], item);
                isSorted = mid;
                return;
            }
            public new bool Remove(T item)
            {
                if (isSorted<Count) Init();
                int l = 0, r = Count, mid = (l + r) / 2;
                while (l < r)
                {
                    mid = (l + r) / 2;
                    if (r == mid) break;
                    else if (comparison(iterators[mid].Value, item) <= 0)
                    {
                        l = mid + 1;
                    }
                    else r = mid;
                }
                if (iterators[mid].Value.Equals(item))
                {
                    base.Remove(iterators[mid]);
                    isSorted = mid;
                    return true;
                }
                else 
                {
                    return false;
                }
            }
            public bool RemoveAt(int index)
            {
                try
                {
                    if (isSorted <= index) Init();
                    base.Remove(iterators[index]);
                    isSorted = index;
                    return true;
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }
            public T this[int index] 
            {
                get 
                {
                    if (isSorted<=index) Init();
                    return iterators[index].Value;
                }
                set 
                {
                    if (isSorted <= index) Init();
                    isSorted = index;
                    RemoveAt(index);
                    Add(value);
                }
            }
            int isSorted = 0;
            void Init()
            {
                LinkedListNode<T> node;
                int index;
                if (isSorted <= 0) 
                {
                    node = First;
                    index = 0;
                }
                else 
                {
                    node = iterators[isSorted - 1].Next; 
                    index = isSorted;
                }
                while (node != null) 
                {
                    if(index < iterators.Count) iterators[index] = node;
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
                    node = First;
                    index = 0;
                }
                else
                {
                    node = iterators[isSorted - 1].Next;
                    index = isSorted;
                }
                while (node != null&&index<=target)
                {
                    if (index < iterators.Count) iterators[index] = node;
                    else iterators.Add(node);
                    node = node.Next;
                    index++;
                }
                isSorted = target+1;
            }
        }
    }
}
