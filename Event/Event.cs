using System.Collections;
using EilansPlugin.Container;

namespace EilansPlugin.Event
{
    // 缓动事件
    public class EasingEvent : IEnumerable
    {
        public PriorityList<EasingEventPartBase> EventParts { get; set; }

        public EasingEvent(double timeLength, double initValue) => Init(timeLength, initValue);

        public EasingEventPartBase this[int index] => EventParts[index];

        public IEnumerator GetEnumerator() => EventParts.GetEnumerator();

        public void Init(double timeLength, double initValue)
        {
            if (timeLength == 0) EventParts = new PriorityList<EasingEventPartBase>((lhs, rhs) => lhs.TimeEnd.CompareTo(rhs.TimeStart)) { };
            else EventParts = new PriorityList<EasingEventPartBase>((lhs, rhs) => lhs.TimeEnd.CompareTo(rhs.TimeStart)) { new EasingEventPartTail(0, initValue) };
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

        // 切分
        public void Divede(double time)
        {
            int i = FindIndex(time);

            EasingEventPartBase[] dividedParts = EventParts[i].Divide(time);
            EventParts.RemoveAt(i);
            EventParts.Add(dividedParts[0]);
            EventParts.Add(dividedParts[1]);
        }

        public EasingEvent Copy()
        {
            EasingEvent newEasingEvent = new EasingEvent(0, 0);
            foreach (EasingEventPartBase part in EventParts)
            {
                if (part is EasingEventPart easingEventPart)
                    newEasingEvent.EventParts.Add(easingEventPart.Copy());
                if (part is EasingEventPartTail easingEventPartTail)
                    newEasingEvent.EventParts.Add(easingEventPartTail.Copy());
            }
            return newEasingEvent;
        }
    }
}
