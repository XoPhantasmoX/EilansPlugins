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
        double TimeStart { get; set; }
        double TimeEnd { get; set; }
        double ValueStart { get; set; }
        double ValueEnd { get; set; }
        double GetValue(double time);
        ICurveEvent[] Divide(double time);
    }

    // 速度事件接口
    public interface ISpeedEvent
    {
        double TimeStart { get; set; }
        double TimeEnd { get; set; }
        double ValueStart { get; set; }
        double ValueEnd { get; set; }
        double Displacement { get; }
        double GetValue(double time);
        double GetDisplacement();
        double GetDisplacement(double time);
        double GetDisplacement(double timeStart, double timeEnd);
        ISpeedEvent[] Divide(double time);
    }

    // 事件集合接口
    public interface IEventCollection : IEnumerable
    {
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
        public double TimeEnd { get; set; }
        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }
        public TransfromType TransformType { get; set; }
        public EaseType EaseType { get; set; }

        public EasingCurveEvent(double timeStart, double timeEnd, double valueStart, double valueEnd, TransfromType transfromType, EaseType easeType)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            TransformType = transfromType;
            EaseType = easeType;
        }

        public double GetValue(double time) =>
            ValueStart + Ease.GetEase(TransformType, EaseType, (time - TimeStart) / (TimeEnd - TimeStart)) * (ValueEnd - ValueStart);

        public ICurveEvent[] Divide(double time)
        {
            double value = GetValue(time);

            return new ICurveEvent[] {
                new EasingCurveEvent(TimeStart, time, ValueStart, value, TransformType, EaseType),
                new EasingCurveEvent(time, TimeEnd, value, ValueEnd, TransformType, EaseType)
            };
        }
    }

    // 空曲线事件
    public struct EmptyCurveEvent : ICurveEvent
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get => double.PositiveInfinity; set { } }
        public double ValueStart { get; set; }
        public double ValueEnd { get => TimeStart; set { } }

        public EmptyCurveEvent(double timeStart, double valueStart)
        {
            TimeStart = timeStart;
            ValueStart = valueStart;
        }

        public double GetValue(double time) => ValueStart;

        public ICurveEvent[] Divide(double time) => new ICurveEvent[]
        {
            new EasingCurveEvent(TimeStart, time, ValueStart, ValueStart, TransfromType.Linear, EaseType.In),
            new EmptyCurveEvent(time, ValueStart)
        };
    }

    // 线性速度事件
    public struct LinearSpeedEvent : ISpeedEvent
    {
        public double TimeStart
        {
            get => _timeStart;
            set
            {
                _timeStart = value;
                UpdateDisplacement();
            }
        }

        public double TimeEnd
        {
            get => _timeEnd;
            set
            {
                _timeEnd = value;
                UpdateDisplacement();
            }
        }

        public double ValueStart
        {
            get => _valueStart;
            set
            {
                _valueStart = value;
                UpdateDisplacement();
            }
        }

        public double ValueEnd
        {
            get => _valueEnd;
            set
            {
                _valueEnd = value;
                UpdateDisplacement();
            }
        }

        public double Displacement => _displacement;

        private double _timeStart;
        private double _timeEnd;
        private double _valueStart;
        private double _valueEnd;
        private double _displacement;

        public LinearSpeedEvent(double timeStart, double timeEnd, double valueStart, double valueEnd)
        {
            _timeStart = timeStart;
            _timeEnd = timeEnd;
            _valueStart = valueStart;
            _valueEnd = valueEnd;
            _displacement = 0;
            UpdateDisplacement();
        }

        public double GetValue(double time) =>
            ValueStart + (time - TimeStart) / (TimeEnd - TimeStart) * (ValueEnd - ValueStart);

        public double GetDisplacement(double timeStart, double timeEnd) =>
            (GetValue(timeStart) + GetValue(timeEnd)) * (timeEnd - timeStart) / 2;

        public double GetDisplacement(double time) => GetDisplacement(TimeStart, time);

        public double GetDisplacement() => GetDisplacement(TimeStart, TimeEnd);

        private void UpdateDisplacement() => _displacement = GetDisplacement(TimeStart, TimeEnd);

        public ISpeedEvent[] Divide(double time)
        {
            double value = GetValue(time);

            return new ISpeedEvent[] {
                new LinearSpeedEvent(TimeStart, time, ValueStart, value),
                new LinearSpeedEvent(time, TimeEnd, value, ValueEnd)
            };
        }
    }

    // 空速度事件
    public struct EmptySpeedEvent : ISpeedEvent
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get => double.PositiveInfinity; set { } }
        public double ValueStart { get; set; }
        public double ValueEnd { get => ValueStart; set { } }
        public double Displacement => double.PositiveInfinity;

        public EmptySpeedEvent(double timeStart, double valueStart)
        {
            TimeStart = timeStart;
            ValueStart = valueStart;
        }

        public double GetValue(double time) => ValueStart;

        public double GetDisplacement(double timeStart, double timeEnd) => (timeEnd - timeStart) * ValueStart;

        public double GetDisplacement(double time) => GetDisplacement(TimeStart, time);

        public double GetDisplacement() => GetDisplacement(TimeStart, TimeEnd);

        public ISpeedEvent[] Divide(double time) => new ISpeedEvent[]
        {
            new LinearSpeedEvent(TimeStart, time, ValueStart, ValueStart),
            new EmptySpeedEvent(time, ValueStart)
        };
    }

    // 缓动事件集合
    public class CurveEventCollection : IEventCollection
    {
        private const int LINEAR_SEARCH_THRESHOLD = 5;

        // 储存事件区块
        // 由于要使用二分查找，需要索引，所以用了List
        public List<ICurveEvent> EventParts { get; set; }

        public CurveEventCollection(double? initValue) => Init(initValue);

        // 索引器
        public ICurveEvent this[int index] => EventParts[index];

        // 迭代器
        public IEnumerator GetEnumerator() => EventParts.GetEnumerator();

        // 初始化
        public void Init(double? initValue)
        {
            if (initValue == null)
                EventParts = new List<ICurveEvent>() { };
            else
                EventParts = new List<ICurveEvent>() { new EmptyCurveEvent(0, (double)initValue) };
        }

        // Sets
        public void SetValueStart(int index, double value) => EventParts[index].ValueStart = value;

        public void SetValueEnd(int index, double value) => EventParts[index].ValueEnd = value;

        // 查找
        public int FindIndex(double time)
        {
            if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

            if (EventParts.Count == 1) return 0;

            if (EventParts.Count <= LINEAR_SEARCH_THRESHOLD)
            {
                for (int i = 0; i < EventParts.Count; i++)
                    if (EventParts[i].TimeStart <= time && EventParts[i].TimeEnd > time)
                        return i;
                return default;
            }

            int j = 0;
            int k = EventParts.Count - 1;

            while (j <= k)
            {
                int m = j + (k - j) / 2;

                if (EventParts[m].TimeStart > time) k = m - 1;
                else if (EventParts[m].TimeEnd < time) j = m + 1;
                else return m;
            }

            return default;
        }

        // 取值
        public double GetValue(double time) =>
            EventParts[FindIndex(time)].GetValue(time);

        // 切分
        public void Divide(double time)
        {
            int i = FindIndex(time);
            ICurveEvent[] dividedParts = EventParts[i].Divide(time);

            EventParts.RemoveAt(i);
            EventParts.Insert(i, dividedParts[1]);
            EventParts.Insert(i, dividedParts[0]);
        }

        // 清空
        public void Clear() => Init(EventParts[0].ValueStart);

        // 复制
        public CurveEventCollection Copy() => new CurveEventCollection(null)
        {
            EventParts = new List<ICurveEvent>(EventParts)
        };
    }

    // 速度事件集合
    public class SpeedEventCollection : IEventCollection
    {
        private const int LINEAR_SEARCH_THRESHOLD = 5;

        // 储存事件区块
        public List<ISpeedEvent> EventParts { get; set; }

        // 位移缓存
        public List<double> DisplacementCache { get; set; }

        public SpeedEventCollection(double? initValue) => Init(initValue);

        // 索引器
        public ISpeedEvent this[int index] => EventParts[index];

        // 迭代器
        public IEnumerator GetEnumerator() => EventParts.GetEnumerator();

        // 初始化
        private void Init(double? initValue)
        {
            if (initValue == null)
            {
                EventParts = new List<ISpeedEvent>() { };
                DisplacementCache = new List<double>() { };
            }
            else
            {
                EventParts = new List<ISpeedEvent>() { new EmptySpeedEvent(0, (double)initValue) };
                DisplacementCache = new List<double>() { 0 };
            }
        }

        // Sets
        public void SetValueStart(int index, double value)
        {
            EventParts[index].ValueStart = value;
            UpdateDisplacementsCache(index + 1, DisplacementCache.Count - 1);
        }

        public void SetValueEnd(int index, double value)
        {
            EventParts[index].ValueEnd = value;
            UpdateDisplacementsCache(index + 1, DisplacementCache.Count - 1);
        }

        // 查找
        public int FindIndex(double time)
        {
            if (time < 0) throw new ArgumentException("\"time\" can't be a negative.");

            if (EventParts.Count == 1) return 0;

            if (EventParts.Count <= LINEAR_SEARCH_THRESHOLD)
            {
                for (int i = 0; i < EventParts.Count; i++)
                    if (EventParts[i].TimeStart <= time && EventParts[i].TimeEnd > time)
                        return i;
                return default;
            }

            int j = 0;
            int k = EventParts.Count - 1;

            while (j <= k)
            {
                int m = j + (k - j) / 2;

                if (EventParts[m].TimeStart > time) k = m - 1;
                else if (EventParts[m].TimeEnd < time) j = m + 1;
                else return m;
            }

            return default;
        }

        // 取值
        public double GetValue(double time) =>
            EventParts[FindIndex(time)].GetValue(time);

        // 获取位移
        public double GetDisplacement(double time)
        {
            int i = FindIndex(time);
            double displacement = 0;

            displacement += DisplacementCache[i] + EventParts[i].GetDisplacement(time);

            return displacement;
        }

        // 更新位移缓存
        public void UpdateDisplacementsCache(int from, int to)
        {
            for (int i = from; i <= to; i++)
                DisplacementCache[i] = DisplacementCache[i - 1] + EventParts[i - 1].GetDisplacement();
        }

        // 切分
        public void Divide(double time)
        {
            int i = FindIndex(time);
            ISpeedEvent[] dividedParts = EventParts[i].Divide(time);

            EventParts.RemoveAt(i);
            EventParts.InsertRange(i, dividedParts);

            DisplacementCache.Add(0);
            UpdateDisplacementsCache(i + 1, DisplacementCache.Count - 1);
        }

        // 清空
        public void Clear() => Init(EventParts[0].ValueStart);

        // 复制
        public SpeedEventCollection Copy() => new SpeedEventCollection(null)
        {
            EventParts = new List<ISpeedEvent>(EventParts),
            DisplacementCache = new List<double>(DisplacementCache)
        };
    }
}
