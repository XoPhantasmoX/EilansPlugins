namespace EilansPlugin
{
    public class TimeManager
    {
        public CurveEventCollection BPMEvent { get; set; }
        public double SecTimer { get; set; }
        public double BPM => GetBPM(SecTimer);
        public double Beat => GetBeat(SecTimer);
        public double BeatTimer => SecTimer / Beat;

        public TimeManager(double initBPM) => BPMEvent = new CurveEventCollection(initBPM);

        public double GetBPM(double secTime) => BPMEvent.GetValue(secTime);

        public double GetBeat(double secTimer) => 60.0 / GetBPM(secTimer);

        public double SecToBeat(double secTime) => secTime / Beat;

        public double BeatToSec(double beatTime) => beatTime * Beat;

        public double RoundBeatTime(double beatTime, int subdivision) =>
            subdivision == 0 ? beatTime : subdivision > 0
            ? (double)(int)(beatTime / subdivision + (beatTime > 0 ? 0.5 : -0.5)) * subdivision
            : (double)(int)(beatTime * subdivision - (beatTime > 0 ? 0.5 : -0.5)) / subdivision;

        public double RoundSecTime(double secTime, int subdivision) =>
            BeatToSec(RoundBeatTime(SecToBeat(secTime), subdivision));
    }
}
