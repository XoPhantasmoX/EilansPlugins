using System;
using System.Collections;
using System.Collections.Generic;

namespace EilansPlugins
{
    namespace Collections
    {
        // 优先列表
        public class PriorityList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
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

            public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

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

            public void CopyTo(Array array, int index) => ((ICollection)list).CopyTo(array, index);
        }

        // 事件列表基类
        public abstract class EventList<T> : IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T> where T : IEvent
        {
            public int Count => Events.Count;
            protected const int LINEAR_SEARCH_THRESHOLD = 12;
            protected List<T> Events { get; set; }

            // 索引器
            public T this[int index]
            {
                get
                {
                    if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                    return Events[index];
                }
            }

            // 迭代器
            public IEnumerator<T> GetEnumerator() => Events.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => Events.GetEnumerator();

            // Gets
            public double GetValueStart(int index)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                return Events[index].ValueStart;
            }

            public double GetValueEnd(int index)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                return Events[index].ValueEnd;
            }

            public double GetTimeStart(int index)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                return Events[index].TimeStart;
            }

            public double GetTimeEnd(int index)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");

                if (index == Count - 1) return double.PositiveInfinity;
                else return Events[index + 1].TimeStart;
            }

            // Sets
            public virtual void SetTimeStart(int index, double time) { }

            public virtual void SetTimeEnd(int index, double time) =>
                SetTimeStart(index + 1, time);

            public virtual void SetValueStart(int index, double value)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                Events[index].ValueStart = value;
            }

            public virtual void SetValueEnd(int index, double value)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                Events[index].ValueEnd = value;
            }

            // 查找
            public int FindIndex(double time)
            {
                if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

                if (Count == 1) return 0;

                if (Count <= LINEAR_SEARCH_THRESHOLD)
                {
                    for (int i = 0; i < Count; i++)
                        if (Events[i].TimeStart <= time && GetTimeEnd(i) > time)
                            return i;
                    return default;
                }

                int j = 0;
                int k = Count - 1;

                while (j <= k)
                {
                    int m = j + (k - j) / 2;

                    if (Events[m].TimeStart > time) k = m - 1;
                    else if (GetTimeEnd(m) < time) j = m + 1;
                    else return m;
                }

                return default;
            }

            // 取值
            public double GetValue(double time)
            {
                if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

                int i = FindIndex(time);
                return Events[i].GetValue(time, GetTimeEnd(i));
            }

            // 切分
            public virtual void Split(double time) { }

            public virtual void Split(params double[] times)
            {
                foreach (double time in times) Split(time);
            }

            // 合并
            public virtual void Merge(int from, int to) { }

            // 清空
            public virtual void Clear() { }
        }

        // 缓动事件列表
        public class CurveEventList : EventList<ICurveEvent>
        {
           public CurveEventList(double initValue) =>
                Events = new List<ICurveEvent>() { new EmptyCurveEvent(0, initValue) };

            private CurveEventList() { }

            // Set
            public override void SetTimeStart(int index, double time)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                if (index == 0) throw new ArgumentException("The first of the event can't be set.");

                if (Events[index] is EasingCurveEvent e)
                    Events[index] = new EasingCurveEvent(time, e.ValueStart, e.ValueEnd, e.TransformType, e.EasingType);
                else
                    Events[index] = new EmptyCurveEvent(time, Events[0].ValueStart);
            }

            // 切分
            public override void Split(double time)
            {
                if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

                int i = FindIndex(time);
                ICurveEvent[] dividedEvents = Events[i].Split(time, GetTimeEnd(i));

                Events.RemoveAt(i);
                Events.Insert(i, dividedEvents[1]);
                Events.Insert(i, dividedEvents[0]);
            }

            // 合并
            public override void Merge(int from, int to)
            {
                if (from < 0 && from >= Count || to < 0 && to >= Count)
                    throw new ArgumentException("Index out of range.");

                if (from > to) throw new ArgumentException("\"from\" can't be bigger than \"to\".");

                ICurveEvent MergedEvent;

                if (Events[from] is EasingCurveEvent e)
                    MergedEvent = new EasingCurveEvent(e.TimeStart, e.ValueStart, Events[to].ValueEnd, e.TransformType, e.EasingType);
                else
                    MergedEvent = new EasingCurveEvent(Events[from].TimeStart, Events[from].ValueStart, Events[to].ValueEnd, TransformType.Linear, EasingType.In);

                Events.RemoveRange(from, to - from + 1);
                Events.Insert(from, MergedEvent);
            }

            // 清空
            public override void Clear() => Events = new List<ICurveEvent>() { new EmptyCurveEvent(0, Events[0].ValueStart) };

            // 复制
            public CurveEventList Copy() => new CurveEventList()
            {
                Events = new List<ICurveEvent>(Events)
            };
        }

        // 速度事件列表
        public class SpeedEventList : EventList<ISpeedEvent>
        {
            public bool DisplacementCacheAutoUpdate { get; set; }
            private List<double> DisplacementCache { get; set; }

            public SpeedEventList(double initValue, bool displacementCacheAutoUpdate = true)
            {
                Events = new List<ISpeedEvent>() { new EmptySpeedEvent(0, initValue) };
                DisplacementCacheAutoUpdate = displacementCacheAutoUpdate;
                DisplacementCache = new List<double>() { 0 };
            }

            private SpeedEventList() { }

            // Sets
            public override  void SetTimeStart(int index, double time)
            {
                if (index < 0 && index >= Count) throw new ArgumentException("Index out of range.");
                if (index == 0) throw new ArgumentException("The first of the event can't be set.");

                if (Events[0] is EasingCurveEvent e)
                    Events[0] = new LinearSpeedEvent(time, e.ValueStart, e.ValueEnd);
                else
                    Events[0] = new EmptySpeedEvent(time, Events[0].ValueStart);

                UpdateDisplacementsCacheAuto(index);
            }

            public override void SetTimeEnd(int index, double time)
            {
                base.SetTimeEnd(index, time);
                UpdateDisplacementsCacheAuto(index + 1);
            }

            public override void SetValueStart(int index, double value)
            {
                base.SetValueStart(index, value);
                UpdateDisplacementsCacheAuto(index + 1);
            }

            public override void SetValueEnd(int index, double value)
            {
                base.SetValueEnd(index, value);
                UpdateDisplacementsCacheAuto(index + 1);
            }

            // 获取位移
            public double GetDisplacement(double time)
            {
                if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

                int i = FindIndex(time);
                double displacement = 0;

                displacement += DisplacementCache[i] + Events[i].GetDisplacement(time, GetTimeEnd(i));

                return displacement;
            }

            // 切分
            public override void Split(double time)
            {
                if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

                int i = FindIndex(time);
                ISpeedEvent[] dividedParts = Events[i].Divide(time, GetTimeEnd(i));

                Events.RemoveAt(i);
                Events.InsertRange(i, dividedParts);

                DisplacementCache.Add(0);
                UpdateDisplacementsCacheAuto(i + 1);
            }

            // 合并
            public override void Merge(int from, int to)
            {
                if (from < 0 && from >= Count || to < 0 && to >= Count)
                    throw new ArgumentException("Index out of range.");

                if (from > to) throw new ArgumentException("\"from\" can't be bigger than \"to\".");

                ISpeedEvent MergedEvent = new LinearSpeedEvent(Events[from].TimeStart, Events[from].ValueStart, Events[to].ValueEnd);

                Events.RemoveRange(from, to - from + 1);
                Events.Insert(from, MergedEvent);

                DisplacementCache.RemoveRange(from + 1, to - from);
                UpdateDisplacementsCacheAuto(from + 1);
            }

            // 清空
            public override void Clear()
            {
                Events = new List<ISpeedEvent>() { new EmptySpeedEvent(0, Events[0].ValueStart) };
                DisplacementCache = new List<double>() { 0 };
            }

            // 复制
            public SpeedEventList Copy() => new SpeedEventList()
            {
                Events = new List<ISpeedEvent>(Events),
                DisplacementCache = new List<double>(DisplacementCache)
            };

            // 更新位移缓存
            public void UpdateDisplacementsCache(int from)
            {
                if (from < 0 && from >= Count) throw new ArgumentException("Index out of range.");

                UpdateDisplacementsCache(from, Count - 1);
            }

            public void UpdateDisplacementsCache(int from, int to)
            {
                if (from < 0 && from >= Count) throw new ArgumentException("Index \"from\" out of range.");
                if (from < 0 && to >= Count) throw new ArgumentException("Index \"to\" out of range.");
                if (from > to) throw new ArgumentException("\"from\" must be greater than \"to\".");

                if (from == 0) DisplacementCache[from++] = 0;
                for (int i = from; i <= to; i++)
                    DisplacementCache[i] = DisplacementCache[i - 1] + Events[i - 1].GetDisplacement(GetTimeEnd(i - 1));
            }

            private void UpdateDisplacementsCacheAuto(int from)
            {
                if (DisplacementCacheAutoUpdate) UpdateDisplacementsCache(from);
            }
        }
    }
}