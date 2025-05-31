using System;
using System.Collections;
using System.Collections.Generic;

// IndexZero 2025/5/30
// 事件

namespace EilansPlugin
{
    // 曲线事件区块接口
    public interface ICurveEventPart
    {
        double TimeStart { get; set; }
        double TimeEnd { get; set; }
        double ValueStart { get; set; }
        double ValueEnd { get; set; }
        double GetValue(double time);
        ICurveEventPart[] Divide(double time);
    }

    // 速度事件区块接口
    public interface ISpeedEventPart
    {
        double TimeStart { get; set; }
        double TimeEnd { get; set; }
        double ValueStart { get; set; }
        double ValueEnd { get; set; }
        double Displacement { get; }
        double GetValue(double time);
        double GetDisplacement(double timeStart, double timeEnd);
        void UpdateDisplacement();
        ISpeedEventPart[] Divide(double time);
    }

    // 缓动事件区块
    public struct EasingCurveEventPart : ICurveEventPart
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get; set; }
        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }
        public TransfromType TransformType { get; set; }
        public EaseType EaseType { get; set; }

        public EasingCurveEventPart(double timeStart, double timeEnd, double valueStart, double valueEnd, TransfromType transfromType, EaseType easeType)
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

        public ICurveEventPart[] Divide(double time)
        {
            double value = GetValue(time);

            return new ICurveEventPart[] {
                new EasingCurveEventPart(TimeStart, time, ValueStart, value, TransformType, EaseType),
                new EasingCurveEventPart(time, TimeEnd, value, ValueEnd, TransformType, EaseType)
            };
        }
    }

    // 空曲线事件
    public struct EmptyCurveEventPart : ICurveEventPart
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get => double.PositiveInfinity; set { } }
        public double ValueStart { get; set; }
        public double ValueEnd { get => TimeStart; set { } }

        public EmptyCurveEventPart(double timeStart, double valueStart)
        {
            TimeStart = timeStart;
            ValueStart = valueStart;
        }

        public double GetValue(double time) => ValueStart;

        public ICurveEventPart[] Divide(double time) => new ICurveEventPart[]
        {
            new EasingCurveEventPart(TimeStart, time, ValueStart, ValueStart, TransfromType.Linear, EaseType.In),
            new EmptyCurveEventPart(time, ValueStart)
        };
    }

    // 线性速度事件
    public struct LinearSpeedEventPart : ISpeedEventPart
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get; set; }
        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }
        public double Displacement => _displacement;

        private double _displacement;

        public LinearSpeedEventPart(double timeStart, double timeEnd, double valueStart, double valueEnd)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            _displacement = 0;
            UpdateDisplacement();
        }

        public double GetValue(double time) =>
            ValueStart + ((time - TimeStart) / (TimeEnd - TimeStart)) * (ValueEnd - ValueStart);

        public double GetDisplacement(double timeStart, double timeEnd) =>
            (GetValue(timeStart) + GetValue(timeEnd)) * (timeEnd - timeStart) / 2;

        public void UpdateDisplacement() => _displacement = GetDisplacement(TimeStart, TimeEnd);

        public ISpeedEventPart[] Divide(double time)
        {
            double value = GetValue(time);

            return new ISpeedEventPart[] {
                new LinearSpeedEventPart(TimeStart, time, ValueStart, value),
                new LinearSpeedEventPart(time, TimeEnd, value, ValueEnd)
            };
        }
    }

    // 空速度事件
    public struct EmptySpeedEventPart : ISpeedEventPart
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get => double.PositiveInfinity; set { } }
        public double ValueStart { get; set; }
        public double ValueEnd { get => ValueStart; set { } }
        public double Displacement => _displacement;

        private double _displacement;

        public EmptySpeedEventPart(double timeStart, double valueStart)
        {
            TimeStart = timeStart;
            ValueStart = valueStart;
            _displacement = 0;
            UpdateDisplacement();
        }

        public double GetValue(double time) => ValueStart;

        public double GetDisplacement(double timeStart, double timeEnd) => (timeEnd - timeStart) * ValueStart;

        public void UpdateDisplacement() => _displacement = GetDisplacement(TimeStart, TimeEnd);

        public ISpeedEventPart[] Divide(double time) => new ISpeedEventPart[]
        {
            new LinearSpeedEventPart(TimeStart, time, ValueStart, ValueStart),
            new EmptySpeedEventPart(time, ValueStart)
        };
    }

    // 缓动事件
    public class CurveEvent : IEnumerable
    {
        // 储存事件区块
        // 由于要使用二分查找，需要索引，所以用了List
        public List<ICurveEventPart> EventParts { get; set; }

        public CurveEvent(double? initValue) => Init(initValue);

        // 索引器
        public ICurveEventPart this[int index] => EventParts[index];

        // 迭代器
        public IEnumerator GetEnumerator() => EventParts.GetEnumerator();

        // 初始化
        public void Init(double? initValue)
        {
            if (initValue == null)
                EventParts = new List<ICurveEventPart>() { };
            else
                EventParts = new List<ICurveEventPart>() { new EmptyCurveEventPart(0, (double)initValue) };
        }

        // 二分查找
        public int FindIndex(double time)
        {
            if (time < 0) throw new ArgumentException("\"time\" can't be a ​​negative.");

            if (EventParts.Count == 1) return 0;

            int i = 0;
            int j = EventParts.Count - 1;

            while (i <= j)
            {
                int m = i + (j - i) / 2;

                if (EventParts[m].TimeStart > time) j = m - 1;
                else if (EventParts[m].TimeEnd < time) i = m + 1;
                else return m;
            }

            return default;
        }

        // 取值
        public double GetValue(double time) =>
            EventParts[FindIndex(time)].GetValue(time);

        // 切分
        public void Divede(double time)
        {
            int i = FindIndex(time);
            ICurveEventPart[] dividedParts = EventParts[i].Divide(time);

            EventParts.RemoveAt(i);
            EventParts.Insert(i, dividedParts[1]);
            EventParts.Insert(i, dividedParts[0]);
        }

        // 清空
        public void Clear() => Init(EventParts[0].ValueStart);

        // 复制
        public CurveEvent Copy() => new CurveEvent(null)
        {
            EventParts = new List<ICurveEventPart>(EventParts)
        };
    }

    // 速度事件
    public class SpeedEvent : IEnumerable
    {
        // 储存事件区块
        public List<ISpeedEventPart> EventParts { get; set; }

        public SpeedEvent(double? initValue) => Init(initValue);

        // 索引器
        public ISpeedEventPart this[int index] => EventParts[index];

        // 迭代器
        public IEnumerator GetEnumerator() => EventParts.GetEnumerator();

        // 初始化
        public void Init(double? initValue)
        {
            if (initValue == null)
                EventParts = new List<ISpeedEventPart>() { };
            else
                EventParts = new List<ISpeedEventPart>() { new EmptySpeedEventPart(0, (double)initValue) };
        }

        // 二分查找
        public int FindIndex(double time)
        {
            if (time < 0) throw new ArgumentException("\"time\" can't be a ​​negative.");

            if (EventParts.Count == 1) return 0;

            int i = 0;
            int j = EventParts.Count - 1;

            while (i <= j)
            {
                int m = i + (j - i) / 2;

                if (EventParts[m].TimeStart > time) j = m - 1;
                else if (EventParts[m].TimeEnd < time) i = m + 1;
                else return m;
            }

            return default;
        }

        // 取值
        public double GetValue(double time) =>
            EventParts[FindIndex(time)].GetValue(time);

        // 切分
        public void Divede(double time)
        {
            int i = FindIndex(time);
            ISpeedEventPart[] dividedParts = EventParts[i].Divide(time);

            EventParts.RemoveAt(i);
            EventParts.Insert(i, dividedParts[1]);
            EventParts.Insert(i, dividedParts[0]);
        }

        // 清空
        public void Clear() => Init(EventParts[0].ValueStart);

        // 复制
        public SpeedEvent Copy() => new SpeedEvent(null)
        {
            EventParts = new List<ISpeedEventPart>(EventParts)
        };
    }
}
