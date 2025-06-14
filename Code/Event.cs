using System;

namespace EilansPlugins
{
    /// <summary>
    /// Event Interface.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The start time of the event.
        /// </summary>
        double TimeStart { get; }

        /// <summary>
        /// The start value of the event.
        /// </summary>
        double ValueStart { get; set; }

        /// <summary>
        /// The end value of the event.
        /// </summary>
        double ValueEnd { get; set; }

        /// <summary>
        /// Get the value of the event.
        /// </summary>
        /// <param name="time">The time when you want to get the value.</param>
        /// <param name="eventTimeEnd">The end time of the event.</param>
        /// <returns>The value of the event.</returns>
        /// <exception cref="ArgumentException"></exception>
        double GetValue(double time, double eventTimeEnd);
    }

    /// <summary>
    /// Curve event Interface.
    /// </summary>
    public interface ICurveEvent : IEvent
    {
        /// <summary>
        /// Split the event.
        /// </summary>
        /// <param name="time">The time when you want to split the event.</param>
        /// <param name="eventTimeEnd">The end time of the event.</param>
        /// <returns>Two split events.</returns>
        ICurveEvent[] Split(double time, double eventTimeEnd);
    }

    /// <summary>
    /// Speed event Interface.
    /// </summary>
    public interface ISpeedEvent : IEvent
    {
        /// <summary>
        /// Gets the displacement of the entire event.
        /// </summary>
        /// <param name="eventTimeEnd">The end time of the event.</param>
        /// <returns>The displacement of the entire event.</returns>
        double GetDisplacement(double eventTimeEnd);

        /// <summary>
        /// Gets the displacement from the beginning to the specified time.
        /// </summary>
        /// <param name="time">The time when you want to get the displacement.</param>
        /// <param name="eventTimeEnd">The end time of the event.</param>
        /// <returns>The displacement from the beginning to the specified time.</returns>
        double GetDisplacement(double time, double eventTimeEnd);

        /// <summary>
        /// Gets the displacement of the specified segment.
        /// </summary>
        /// <param name="timeStart">The start time when you want to get the displacement.</param>
        /// <param name="timeEnd">The end time when you want to get the displacement.</param>
        /// <param name="eventTimeEnd">The end time of the event.</param>
        /// <returns>The displacement of the specified segment.</returns>
        double GetDisplacement(double timeStart, double timeEnd, double eventTimeEnd);

        /// <summary>
        /// Split the event.
        /// </summary>
        /// <param name="time">The time when you want to split the event.</param>
        /// <param name="eventTimeEnd">The end time of the event.</param>
        /// <returns>Two split events.</returns>
        ISpeedEvent[] Divide(double time, double eventTimeEnd);
    }

    /// <summary>
    /// Easing curve event.
    /// </summary>
    public class EasingCurveEvent : ICurveEvent
    {
        public double TimeStart { get; set; }
        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }

        /// <summary>
        /// The type of transition.
        /// </summary>
        public TransformType TransformType { get; set; }

        /// <summary>
        /// The type of easing.
        /// </summary>
        public EasingType EasingType { get; set; }

        public EasingCurveEvent(double timeStart, double valueStart, double valueEnd, TransformType transfromType, EasingType easeType)
        {
            TimeStart = timeStart;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            TransformType = transfromType;
            EasingType = easeType;
        }

        public double GetValue(double time, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (time < TimeStart || time > eventTimeEnd)
                throw new ArgumentException("time must be between TimeStart and eventTimeEnd (inclusive).");

            return ValueStart + Ease.GetEase(TransformType, EasingType, (time - TimeStart) / (eventTimeEnd - TimeStart)) * (ValueEnd - ValueStart);
        }

        public ICurveEvent[] Split(double time, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (time < TimeStart || time > eventTimeEnd)
                throw new ArgumentException("time must be between TimeStart and eventTimeEnd (inclusive).");

            double value = GetValue(time, eventTimeEnd);

            return new ICurveEvent[]
            {
                new EasingCurveEvent(TimeStart, ValueStart, value, TransformType, EasingType),
                new EasingCurveEvent(time, value, ValueEnd, TransformType, EasingType)
            };
        }
    }

    /// <summary>
    /// Empty curve event, with an infinite end time.
    /// </summary>
    public class EmptyCurveEvent : ICurveEvent
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

        public double GetValue(double time, double _ = 0)
        {
            if (time < TimeStart)
                throw new ArgumentException("time can't be smaller than TimeStart.");

            return ValueStart;
        }

        public ICurveEvent[] Split(double time, double _ = 0)
        {
            if (time < TimeStart)
                throw new ArgumentException("time can't be smaller than TimeStart.");

            return new ICurveEvent[]
            {
                new EasingCurveEvent(TimeStart, ValueStart, ValueStart, TransformType.Linear, EasingType.In),
                new EmptyCurveEvent(time, ValueStart)
            };
        }
    }

    /// <summary>
    /// Linear speed event.
    /// </summary>
    public class LinearSpeedEvent : ISpeedEvent
    {
        private readonly double _timeStart;

        public double TimeStart => _timeStart;

        public double ValueStart { get; set; }

        public double ValueEnd { get; set; }

        public LinearSpeedEvent(double timeStart, double valueStart, double valueEnd)
        {
            _timeStart = timeStart;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
        }

        public double GetValue(double time, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (time < TimeStart || time > eventTimeEnd)
                throw new ArgumentException("time must be between TimeStart and eventTimeEnd (inclusive).");

            return ValueStart + (time - TimeStart) / (eventTimeEnd - TimeStart) * (ValueEnd - ValueStart);
        }

        public double GetDisplacement(double timeStart, double timeEnd, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (timeStart < TimeStart || timeStart > eventTimeEnd)
                throw new ArgumentException("timeStart must be between TimeStart and eventTimeEnd (inclusive).");
            if (timeEnd < TimeStart || timeEnd > eventTimeEnd)
                throw new ArgumentException("timeEnd must be between TimeStart and eventTimeEnd (inclusive).");
            if (timeEnd < timeStart)
                throw new ArgumentException("timeEnd must be greater than timeStart.");

            return (GetValue(timeStart, eventTimeEnd) + GetValue(timeEnd, eventTimeEnd)) * (timeEnd - timeStart) / 2;
        }

        public double GetDisplacement(double time, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (time < TimeStart || time > eventTimeEnd)
                throw new ArgumentException("time must be between TimeStart and eventTimeEnd (inclusive).");

            return GetDisplacement(TimeStart, time, eventTimeEnd);
        }

        public double GetDisplacement(double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");

            return GetDisplacement(TimeStart, eventTimeEnd, eventTimeEnd);
        }

        public ISpeedEvent[] Divide(double time, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (time < TimeStart || time > eventTimeEnd)
                throw new ArgumentException("time must be between TimeStart and eventTimeEnd (inclusive).");

            double value = GetValue(time, eventTimeEnd);

            return new ISpeedEvent[] {
                new LinearSpeedEvent(TimeStart, ValueStart, value),
                new LinearSpeedEvent(time, value, ValueEnd)
            };
        }
    }

    /// <summary>
    /// Empty speed event, with an infinite end time.
    /// </summary>
    public class EmptySpeedEvent : ISpeedEvent
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

        public double GetValue(double time, double _ = 0)
        {
            if (time < TimeStart)
                throw new ArgumentException("time must be between TimeStart and eventTimeEnd (inclusive).");

            return ValueStart;
        }

        public double GetDisplacement(double timeStart, double timeEnd, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (timeStart < TimeStart || timeStart > eventTimeEnd)
                throw new ArgumentException("timeStart must be between TimeStart and eventTimeEnd (inclusive).");
            if (timeEnd < TimeStart || timeEnd > eventTimeEnd)
                throw new ArgumentException("timeEnd must be between TimeStart and eventTimeEnd (inclusive).");
            if (timeEnd < timeStart)
                throw new ArgumentException("timeEnd must be greater than timeStart.");

            return (timeEnd - timeStart) * ValueStart;
        }

        public double GetDisplacement(double time, double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");
            if (time < TimeStart || time > eventTimeEnd)
                throw new ArgumentException("time must be between TimeStart and eventTimeEnd (inclusive).");

            return GetDisplacement(TimeStart, time, eventTimeEnd);
        }

        public double GetDisplacement(double eventTimeEnd)
        {
            if (eventTimeEnd <= TimeStart)
                throw new ArgumentException("eventTimeEnd must be greater than TimeStart.");

            return GetDisplacement(TimeStart, eventTimeEnd, eventTimeEnd);
        }

        public ISpeedEvent[] Divide(double time, double _ = 0)
        {
            if (time < TimeStart)
                throw new ArgumentException("time can't be smaller than TimeStart.");

            return new ISpeedEvent[]
            {
                new LinearSpeedEvent(TimeStart, ValueStart, ValueStart),
                new EmptySpeedEvent(time, ValueStart)
            };
        }
    }
}