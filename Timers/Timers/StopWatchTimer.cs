namespace Utilities.Timers
{
    /// <summary>
    /// A stopwatch timer class that extends the base Timer class to function as a stopwatch.
    /// </summary>
    public class StopWatchTimer : Timer
    {
        /// <summary>
        /// Initializes a new instance of the StopWatchTimer class with an initial time of 0.
        /// </summary>
        public StopWatchTimer() : base(0f) { }

        /// <summary>
        /// Updates the timer by adding the delta time to the current time if the timer is running.
        /// This method needs to be called in an update loop to function properly.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame (in seconds).</param>
        public override void Tick(float deltaTime)
        {
            if (IsRunning)
            {
                Time += deltaTime;
            }
        }

        /// <summary>
        /// Resets the stopwatch timer to zero.
        /// </summary>
        public void Reset() => Time = 0;

        /// <summary>
        /// Retrieves the current time value of the stopwatch.
        /// </summary>
        /// <returns>The current time of the stopwatch.</returns>
        public float GetTime() => Time;
    }
}