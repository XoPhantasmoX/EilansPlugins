namespace EilansPlugins.Event
{
    // 缓动事件区块基类
    public abstract class EventPartBase
    {
        public double TimeStart { get; set; }
        public double TimeEnd { get; set; }
        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }
        public abstract double GetValue(double time);
        public abstract EventPartBase[] Divide(double time);
    }

    // 普通缓动事件区块
    public class EasingEventPart : EventPartBase
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

        public override EventPartBase[] Divide(double time)
        {
            double value = GetValue(time);

            return new EventPartBase[] {
                new EasingEventPart(TimeStart, time, ValueStart, value, EaseType),
                new EasingEventPart(time, TimeEnd, value, ValueEnd, EaseType)
            };
        }

        public EasingEventPart Copy() => new EasingEventPart(TimeStart, TimeEnd, ValueStart, ValueEnd, EaseType);
    }

    // 缓动事件尾区块，只有一个固定的值和起始时间，结束时间无限大
    public class EasingEventPartTail : EventPartBase
    {
        public double Value { get; set; }
        public new double TimeStart { get; set; }
        public new double TimeEnd { get => double.PositiveInfinity; set { } }
        public new double ValueStart { get => Value; set { } }
        public new double ValueEnd { get => Value; set { } }

        public EasingEventPartTail(double timeStart, double value)
        {
            TimeStart = timeStart;
            Value = value;
        }

        public override double GetValue(double time) => ValueStart;

        public override EventPartBase[] Divide(double time)
        {
            double value = GetValue(time);

            return new EventPartBase[] {
                new EasingEventPart(TimeStart, time, ValueStart, value, EaseType.Linear),
                new EasingEventPartTail(time, value)
            };
        }

        public EasingEventPartTail Copy() => new EasingEventPartTail(TimeStart, Value);
    }
}