using System.Collections;
using System.Collections.Generic;

namespace EilansPlugins
{
    public abstract class EventPart
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get; set; }
        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }
        public abstract double GetValue(double time);
        public abstract EasingEventPartBase[] Divide(double time);
    }

    public abstract class EasingEventPartBase : EventPart { }

    public class EasingEventPart : EasingEventPartBase
    {
        public new double TimeStart { get; set; }
        public new double TimeEnd { get; set; }
        public new double ValueStart { get; set; }
        public new double ValueEnd { get; set; }
        public EaseType EaseType { get; set; }

        public EasingEventPart(double timeStart, double timeEnd, double valueStart, double valueEnd, EaseType easeType)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            EaseType = easeType;
        }

        public override double GetValue(double time) => ValueStart + Ease.GetEase(EaseType, (time - TimeStart) / (TimeEnd - TimeStart)) * (ValueEnd - ValueStart);

        public override EasingEventPartBase[] Divide(double time)
        {
            double value = GetValue(time);

            return new EasingEventPartBase[] {
                new EasingEventPart(TimeStart, time, ValueStart, value, EaseType),
                new EasingEventPart(time, TimeEnd, value, ValueEnd, EaseType)
            };
        }

        public EasingEventPart Copy() => new EasingEventPart(TimeStart, TimeEnd, ValueStart, ValueEnd, EaseType);
    }

    public class EasingEventPartTail : EasingEventPartBase
    {
        public double Value { get; set; }
        public new double TimeStart { get; set; }
        public new double ValueStart { get => Value; set { } }
        public new double TimeEnd { get => double.PositiveInfinity; set { } }
        public new double ValueEnd { get => double.PositiveInfinity; set { } }

        public EasingEventPartTail(double timeStart, double value)
        {
            TimeStart = timeStart;
            Value = value;
        }

        public override double GetValue(double time) => ValueStart;

        public override EasingEventPartBase[] Divide(double time)
        {
            double value = GetValue(time);

            return new EasingEventPartBase[] {
                new EasingEventPart(TimeStart, time, ValueStart, value, EaseType.Linear),
                new EasingEventPartTail(time, value)
            };
        }

        public EasingEventPart Copy() => new EasingEventPart(TimeStart, TimeEnd, ValueStart, ValueEnd, EaseType);
    }

    public class EasingEvent : IEnumerable
    {
        public List<EasingEventPartBase> EventParts { get; set; }

        public EasingEvent(double timeLength, double initValue) => Init(timeLength, initValue);

        public EasingEventPartBase this[int index] => EventParts[index];

        public IEnumerator GetEnumerator() => EventParts.GetEnumerator();

        public void Init(double timeLength, double initValue)
        {
            if (timeLength == 0) EventParts = new List<EasingEventPartBase> { };
            else EventParts = new List<EasingEventPartBase> { new EasingEventPartTail(0, initValue) };
        }

        public int FindIndex(double time)
        {
            if (EventParts.Count == 1) return 0;

            // 二分查找
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

        public double GetValue(double time) => EventParts[FindIndex(time)].GetValue(time);

        public void Divede(double time)
        {
            int i = FindIndex(time);

            EasingEventPartBase[] dividedParts = EventParts[i].Divide(time);
            EventParts[i] = dividedParts[1];
            EventParts.Insert(i, dividedParts[0]);
        }

        public EasingEvent Copy()
        {
            EasingEvent newEasingEvent = new EasingEvent(0, 0);
            foreach (EasingEventPartBase part in EventParts)
            {
                if (part is EasingEventPart easingEventPart) newEasingEvent.EventParts.Add(easingEventPart.Copy());
            }
            return newEasingEvent;
        }
    }
}
