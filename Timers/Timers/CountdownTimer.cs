using System;

namespace Utilities.Timers
{
    /// <summary>
    /// A countdown timer class that extends the base Timer class to function as a countdown.
    /// It counts down from a specified time and invokes an event when the countdown reaches zero.
    /// </summary>
    [Serializable]
    public class CountdownTimer : Timer
    {
        /// <summary>
        /// Event triggered when the countdown finishes.
        /// If configured, it also stops the countdown timer when triggered.
        /// Note: This event is not triggered when the timer is manually stopped.
        /// </summary>
        public Action OnTimeEnd = delegate { };

        /// <summary>
        /// Initializes a new instance of the CountdownTimer class with an initial countdown time.
        /// Optionally, the timer can be set to stop when the countdown ends.
        /// </summary>
        /// <param name="value">The initial countdown time in seconds.</param>
        /// <param name="callStopOnTimeEnd">If true, the timer will automatically stop when the countdown ends.</param>
        public CountdownTimer(float value, bool callStopOnTimeEnd = true) : base(value) 
        {
            if(callStopOnTimeEnd) OnTimeEnd += Stop;
        }

        /// <summary>
        /// Updates the countdown timer by subtracting the delta time from the current time.
        /// If the countdown reaches zero, it triggers the OnTimeEnd event.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame update.</param>
        public override void Tick(float deltaTime)
        {
            if (IsRunning)
            {
                if (!IsFinished)
                    Time -= deltaTime;
                else
                {
                    Time = 0;
                    OnTimeEnd.Invoke();
                }
            }
        }

        /// <summary>
        /// Checks if the countdown has finished by comparing if the current time is less than or equal to zero.
        /// </summary>
        public bool IsFinished => Time <= 0;

        /// <summary>
        /// Resets the countdown to the initial time set during construction.
        /// </summary>
        public void Reset() => Time = initialTime;

        /// <summary>
        /// Resets the countdown with a new specified time.
        /// </summary>
        /// <param name="newTime">The new time to set as the countdown time.</param>
        public void Reset(float newTime)
        {
            initialTime = newTime;
            Reset();
        }
    }
}
