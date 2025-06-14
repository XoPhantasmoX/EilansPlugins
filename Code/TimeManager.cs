using EilansPlugins.Collections;

namespace EilansPlugins
{
    public class TimeManager
    {
        /// <summary>
        /// BPM easing event.
        /// </summary>
        public CurveEventList BPMEvent { get; set; }

        /// <summary>
        /// The timer in seconds, calculations will be based on it.
        /// </summary>
        public double SecTimer { get; set; }

        /// <summary>
        /// The BPM from BPM event.
        /// </summary>
        public double BPM => GetBPM(SecTimer);

        /// <summary>
        /// The beat from BPM event.
        /// </summary>
        public double Beat => GetBeat(SecTimer);

        /// <summary>
        /// The beat time from BPM event.
        /// </summary>
        public double BeatTimer => SecTimer / Beat;

        public TimeManager(double initBPM) => BPMEvent = new CurveEventList(initBPM);

        /// <summary>
        /// Get BPM from the BPM event.
        /// </summary>
        /// <param name="secTime">The time when you want to get the BPM.</param>
        /// <returns>BPM from the BPM event.</returns>
        public double GetBPM(double secTime) => BPMEvent.GetValue(secTime);

        /// <summary>
        /// Get beat from BPM event.
        /// </summary>
        /// <param name="secTime">The time when you want to get the beat.</param>
        /// <returns>Beat from the BPM event.</returns>
        public double GetBeat(double secTimer) => 60.0 / GetBPM(secTimer);

        /// <summary>
        /// Convert seconds to beats.
        /// </summary>
        /// <param name="secTime">The seconds time.</param>
        /// <returns>The beat time.</returns>
        public double SecToBeat(double secTime) => secTime / Beat;

        /// <summary>
        /// Convert beats to seconds.
        /// </summary>
        /// <param name="beatTime">The beats time.</param>
        /// <returns>The seconds time.</returns>
        public double BeatToSec(double beatTime) => beatTime * Beat;

        /// <summary>
        /// Rounds a beats time value to the nearest multiple of the specified subdivision.
        /// </summary>
        /// <param name="beatTime">The beats time value to round</param>
        /// <param name="subdivision">
        /// Rounding granularity:<br></br>
        /// <list type="bullet">
        /// <item>subdivision > 0 : Defines fractional beat intervals (1/subdivision)</item>
        /// <item>subdivision &lt; 0 : Defines full-beat intervals (|subdivision|)</item>
        /// <item>subdivision = 0 : No rounding</item>
        /// </list>
        /// </param>
        /// <returns>
        /// The rounded beats time aligned to the nearest subdivision multiple.
        /// </returns>
        public double RoundBeatTime(double beatTime, int subdivision) =>
            subdivision == 0 ? beatTime : subdivision > 0
            ? (double)(int)(beatTime / subdivision + (beatTime > 0 ? 0.5 : -0.5)) * subdivision
            : (double)(int)(beatTime * subdivision - (beatTime > 0 ? 0.5 : -0.5)) / subdivision;

        /// <summary>
        /// Rounds a seconds time value to the nearest multiple of the specified subdivision.
        /// </summary>
        /// <param name="secTime">The seconds time value to round</param>
        /// <param name="subdivision">
        /// Rounding granularity:<br></br>
        /// <list type="bullet">
        /// <item>subdivision > 0 : Defines fractional beat intervals (1/subdivision)</item>
        /// <item>subdivision &lt; 0 : Defines full-beat intervals (|subdivision|)</item>
        /// <item>subdivision = 0 : No rounding</item>
        /// </list>
        /// </param>
        /// <returns>
        /// The rounded seconds time aligned to the nearest subdivision multiple.
        /// </returns>
        public double RoundSecTime(double secTime, int subdivision) =>
            BeatToSec(RoundBeatTime(SecToBeat(secTime), subdivision));
    }
}