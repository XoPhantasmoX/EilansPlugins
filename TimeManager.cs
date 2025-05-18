using System.Collections.Generic;
using System.Linq;

namespace EilansPlugins
{
    public readonly struct SecTime
    {
        private readonly double _value;

        public SecTime(double value) => _value = value;

        // 运算
        public static implicit operator double(SecTime secTime) => secTime._value;
        public static implicit operator SecTime(double value) => new SecTime(value);
        public static implicit operator SecTimeF(SecTime secTime) => new SecTimeF((float)secTime);

        public BeatTime ToBeatTime(double beat) => _value / beat;
    }

    public readonly struct SecTimeF
    {
        private readonly float _value;

        public SecTimeF(float value) => _value = value;

        // 运算
        public static implicit operator float(SecTimeF secTime) => secTime._value;
        public static implicit operator SecTimeF(float value) => new SecTimeF(value);
        public static implicit operator SecTime(SecTimeF secTimeF) => new SecTime(secTimeF);

        public BeatTimeF ToSecTimeF(double beat) => (float)(_value / beat);
    }

    public readonly struct BeatTime
    {
        private readonly double _value;

        public BeatTime(double value) => _value = value;

        // 运算
        public static implicit operator double(BeatTime secTime) => secTime._value;
        public static implicit operator BeatTime(double value) => new BeatTime(value);
        public static implicit operator BeatTimeF(BeatTime beatTime) => new BeatTimeF((float)beatTime);

        public SecTime ToSecTime(double beat) => _value * beat;
    }

    public readonly struct BeatTimeF
    {
        private readonly float _value;

        public BeatTimeF(float value) => _value = value;

        // 运算
        public static implicit operator float(BeatTimeF secTimeF) => (float)secTimeF._value;
        public static implicit operator BeatTimeF(float value) => new BeatTimeF(value);
        public static implicit operator BeatTime(BeatTimeF beatTimeF) => new BeatTime(beatTimeF);

        public SecTimeF ToSecTimeF(double beat) => (float)(_value * beat);
    }

    public class BPMList
    {
        private SortedList<BeatTime, double> _list = new SortedList<BeatTime, double>();

        public void Add(BeatTime time, double bpm) => _list.Add(time, bpm);

        // 线性查找，待优化
        public double GetBPM(BeatTime time) => (_list.First((t) => time > t.Key)).Value;

        public void Clear() => _list = new SortedList<BeatTime, double>();
    }

    public class TimeManager
    {
        public BPMList BPMList { get; set; } = new BPMList();
        public double BPM => BPMList.GetBPM(BeatTimer);
        public double Beat => 60.0 / BPM;
        public BeatTime BeatTimer { get; set; } = 0;
        public BeatTimeF BeatTimerF => BeatTimer;
        public SecTime SecTimer => BeatTimer.ToSecTime(Beat);
        public SecTimeF SecTimerF => SecTimer;

        public TimeManager(double initBpm) => BPMList.Add(0, initBpm);
    }
}
