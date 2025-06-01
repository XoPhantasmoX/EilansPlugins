using System;
using System.Collections;
using System.Collections.Generic;

// IndexZero 2025/5/30
// 事件

namespace EilansPlugin
{
    // 曲线事件接口
    public interface ICurveEvent
    {
        double TimeStart { get; }
        double ValueStart { get; set; }
        double ValueEnd { get; set; }
        double GetValue(double time, double timeEnd);
        ICurveEvent[] Divide(double time, double timeEnd);
    }

    // 速度事件接口
    public interface ISpeedEvent
    {
        double TimeStart { get; }
        double ValueStart { get; set; }
        double ValueEnd { get; set; }
        double GetValue(double time, double timeEnd);
        double GetDisplacement(double eventTimeEnd);
        double GetDisplacement(double time, double eventTimeEnd);
        double GetDisplacement(double timeStart, double timeEnd, double eventTimeEnd);
        ISpeedEvent[] Divide(double time, double timeEnd);
    }

    // 事件集合接口
    public interface IEventCollection : IEnumerable
    {
        void SetTimeStart(int index, double time);
        void SetValueStart(int index, double value);
        void SetValueEnd(int index, double value);
        int FindIndex(double time);
        double GetValue(double time);
        void Divide(double time);
        void Clear();
    }

    // 缓动事件
    public struct EasingCurveEvent : ICurveEvent
    {
        public double TimeStart { get; set; }
        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }
        public TransfromType TransformType { get; set; }
        public EaseType EaseType { get; set; }

        public EasingCurveEvent(double timeStart, double valueStart, double valueEnd, TransfromType transfromType, EaseType easeType)
        {
            TimeStart = timeStart;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            TransformType = transfromType;
            EaseType = easeType;
        }

        public double GetValue(double time, double timeEnd) =>
            ValueStart + Ease.GetEase(TransformType, EaseType, (time - TimeStart) / (timeEnd - TimeStart)) * (ValueEnd - ValueStart);

        public ICurveEvent[] Divide(double time, double timeEnd)
        {
            double value = GetValue(time, timeEnd);

            return new ICurveEvent[] {
                new EasingCurveEvent(TimeStart, ValueStart, value, TransformType, EaseType),
                new EasingCurveEvent(time, value, ValueEnd, TransformType, EaseType)
            };
        }
    }

    // 空曲线事件
    public struct EmptyCurveEvent : ICurveEvent
    {
        private double _timeStart;

        public double TimeStart => _timeStart;
        public double ValueStart { get; set; }
        public double ValueEnd { get => ValueStart; set => ValueStart = value; }

        public EmptyCurveEvent(double timeStart, double valueStart)
        {
            _timeStart = timeStart;
            ValueStart = valueStart;
        }

        public double GetValue(double time, double timeEnd) => ValueStart;

        public ICurveEvent[] Divide(double time, double timeEnd) => new ICurveEvent[]
        {
            new EasingCurveEvent(TimeStart, ValueStart, ValueStart, TransfromType.Linear, EaseType.In),
            new EmptyCurveEvent(time, ValueStart)
        };
    }

    // 线性速度事件
    public struct LinearSpeedEvent : ISpeedEvent
    {
        private double _timeStart;

        public double TimeStart => _timeStart;

        public double ValueStart { get; set; }

        public double ValueEnd { get; set; }

        public LinearSpeedEvent(double timeStart, double valueStart, double valueEnd)
        {
            _timeStart = timeStart;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
        }

        public double GetValue(double time, double timeEnd) =>
            ValueStart + (time - TimeStart) / (timeEnd - TimeStart) * (ValueEnd - ValueStart);

        public double GetDisplacement(double timeStart, double timeEnd, double eventTimeEnd) =>
            (GetValue(timeStart, eventTimeEnd) + GetValue(timeEnd, eventTimeEnd)) * (timeEnd - timeStart) / 2;

        public double GetDisplacement(double time, double eventTimeEnd) => GetDisplacement(TimeStart, time, eventTimeEnd);

        public double GetDisplacement(double eventTimeEnd) => GetDisplacement(TimeStart, eventTimeEnd, eventTimeEnd);

        public ISpeedEvent[] Divide(double time, double timeEnd)
        {
            double value = GetValue(time, timeEnd);

            return new ISpeedEvent[] {
                new LinearSpeedEvent(TimeStart, ValueStart, value),
                new LinearSpeedEvent(time, value, ValueEnd)
            };
        }
    }

    // 空速度事件
    public struct EmptySpeedEvent : ISpeedEvent
    {
        private double _timeStart;

        public double TimeStart => _timeStart;
        public double ValueStart { get; set; }
        public double ValueEnd { get => ValueStart; set => ValueStart = value; }

        public EmptySpeedEvent(double timeStart, double valueStart)
        {
            _timeStart = timeStart;
            ValueStart = valueStart;
        }

        public double GetValue(double time, double timeEnd) => ValueStart;

        public double GetDisplacement(double timeStart, double timeEnd, double eventTimeEnd) => (timeEnd - timeStart) * ValueStart;

        public double GetDisplacement(double time, double eventTimeEnd) => GetDisplacement(TimeStart, time, eventTimeEnd);

        public double GetDisplacement(double eventTimeEnd) => GetDisplacement(TimeStart, eventTimeEnd);

        public ISpeedEvent[] Divide(double time, double _) => new ISpeedEvent[]
        {
            new LinearSpeedEvent(TimeStart, ValueStart, ValueStart),
            new EmptySpeedEvent(time, ValueStart)
        };
    }

    // 缓动事件集合
    public class CurveEventCollection : IEventCollection
    {
        private const int LINEAR_SEARCH_THRESHOLD = 5;
        private List<ICurveEvent> Events { get; set; }

        public CurveEventCollection(double initValue) =>
            Events = new List<ICurveEvent>() { new EmptyCurveEvent(0, initValue) };

        private CurveEventCollection() { }

        // 索引器
        public ICurveEvent this[int index] => Events[index];

        // 迭代器
        public IEnumerator GetEnumerator() => Events.GetEnumerator();

        // Sets
        public void SetTimeStart(int index, double time)
        {
            if (index < 0 && index >= Events.Count) throw new ArgumentException("Index out of range.");
            if (index == 0) throw new ArgumentException("The first of the event can't be set.");

            Merge(index - 1, index);
            Divide(time);
        }

        public void SetValueStart(int index, double value)
        {
            if (index < 0 && index >= Events.Count) throw new ArgumentException("Index out of range.");
            Events[index].ValueStart = value;
        }

        public void SetValueEnd(int index, double value)
        {
            if (index < 0 && index >= Events.Count) throw new ArgumentException("Index out of range.");
            Events[index].ValueEnd = value;
        }

        // 查找
        public int FindIndex(double time)
        {
            if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

            if (Events.Count == 1) return 0;

            if (Events.Count <= LINEAR_SEARCH_THRESHOLD)
            {
                for (int i = 0; i < Events.Count; i++)
                    if (Events[i].TimeStart <= time && GetTimeEnd(i) > time)
                        return i;
                return default;
            }

            int j = 0;
            int k = Events.Count - 1;

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
            int i = FindIndex(time);
            return Events[i].GetValue(time, GetTimeEnd(i));
        }

        // 切分
        public void Divide(double time)
        {
            int i = FindIndex(time);
            ICurveEvent[] dividedEvents = Events[i].Divide(time, GetTimeEnd(i));

            Events.RemoveAt(i);
            Events.Insert(i, dividedEvents[1]);
            Events.Insert(i, dividedEvents[0]);
        }

        // 合并
        public void Merge(int from, int to)
        {

            if (from < 0 && from >= Events.Count || to < 0 && to >= Events.Count)
                throw new ArgumentException("Index out of range.");

            ICurveEvent MergedEvent = new EasingCurveEvent(Events[from].TimeStart, Events[from].ValueStart, Events[to].ValueEnd, TransfromType.Linear, EaseType.In);

            Events.RemoveRange(from, to - from + 1);
            Events.Insert(from, MergedEvent);
        }

        // 清空
        public void Clear() => Events = new List<ICurveEvent>() { new EmptyCurveEvent(0, Events[0].ValueStart) };

        // 复制
        public CurveEventCollection Copy() => new CurveEventCollection()
        {
            Events = new List<ICurveEvent>(Events)
        };

        private double GetTimeEnd(int index)
        {
            if (index == Events.Count - 1) return double.PositiveInfinity;
            else return Events[index + 1].TimeStart;
        }
    }

    // 速度事件集合
    public class SpeedEventCollection : IEventCollection
    {
        private const int LINEAR_SEARCH_THRESHOLD = 5;
        private List<ISpeedEvent> Events { get; set; }
        private List<double> DisplacementCache { get; set; }

        public SpeedEventCollection(double initValue)
        {
            Events = new List<ISpeedEvent>() { new EmptySpeedEvent(0, initValue) };
            DisplacementCache = new List<double>() { 0 };
        }

        private SpeedEventCollection() { }

        // 索引器
        public ISpeedEvent this[int index] => Events[index];

        // 迭代器
        public IEnumerator GetEnumerator() => Events.GetEnumerator();

        // Sets
        public void SetTimeStart(int index, double time)
        {
            if (index < 0 && index >= Events.Count) throw new ArgumentException("Index out of range.");
            if (index == 0) throw new ArgumentException("The first of the event can't be set.");

            Merge(index - 1, index);
            Divide(time);

            UpdateDisplacementsCache(index);
        }

        public void SetValueStart(int index, double value)
        {
            Events[index].ValueStart = value;
            UpdateDisplacementsCache(index + 1);
        }

        public void SetValueEnd(int index, double value)
        {
            Events[index].ValueEnd = value;
            UpdateDisplacementsCache(index + 1);
        }

        // 查找
        public int FindIndex(double time)
        {
            if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

            if (Events.Count == 1) return 0;

            if (Events.Count <= LINEAR_SEARCH_THRESHOLD)
            {
                for (int i = 0; i < Events.Count; i++)
                    if (Events[i].TimeStart <= time && GetTimeEnd(i) > time)
                        return i;
                return default;
            }

            int j = 0;
            int k = Events.Count - 1;

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
            int i = FindIndex(time);
            return Events[i].GetValue(time, GetTimeEnd(i));
        }

        // 获取位移
        public double GetDisplacement(double time)
        {
            int i = FindIndex(time);
            double displacement = 0;

            displacement += DisplacementCache[i] + Events[i].GetDisplacement(time, GetTimeEnd(i));

            return displacement;
        }

        // 更新位移缓存
        private void UpdateDisplacementsCache(int from)
        {
            if (from == 0) DisplacementCache[from++] = 0;
            for (int i = from; i < DisplacementCache.Count; i++)
                DisplacementCache[i] = DisplacementCache[i - 1] + Events[i - 1].GetDisplacement(GetTimeEnd(i - 1));
        }

        // 切分
        public void Divide(double time)
        {
            int i = FindIndex(time);
            ISpeedEvent[] dividedParts = Events[i].Divide(time, GetTimeEnd(i));

            Events.RemoveAt(i);
            Events.InsertRange(i, dividedParts);

            DisplacementCache.Add(0);
            UpdateDisplacementsCache(i + 1);
        }

        // 合并
        public void Merge(int from, int to)
        {

            if (from < 0 && from >= Events.Count || to < 0 && to >= Events.Count)
                throw new ArgumentException("Index out of range.");

            ISpeedEvent MergedEvent = new LinearSpeedEvent(Events[from].TimeStart, Events[from].ValueStart, Events[to].ValueEnd);

            Events.RemoveRange(from, to - from + 1);
            Events.Insert(from, MergedEvent);

            DisplacementCache.RemoveRange(from + 1, to - from);
            UpdateDisplacementsCache(from + 1);
        }

        // 清空
        public void Clear()
        {
            Events = new List<ISpeedEvent>() { new EmptySpeedEvent(0, Events[0].ValueStart) };
            DisplacementCache = new List<double>() { 0 };
        }

        // 复制
        public SpeedEventCollection Copy() => new SpeedEventCollection()
        {
            Events = new List<ISpeedEvent>(Events),
            DisplacementCache = new List<double>(DisplacementCache)
        };

        private double GetTimeEnd(int index)
        {
            if (index == Events.Count - 1) return double.PositiveInfinity;
            else return Events[index + 1].TimeStart;
        }
    }
}
