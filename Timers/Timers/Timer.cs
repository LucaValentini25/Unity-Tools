using System;
using UnityEngine;

namespace Utilities.Timers
{
    /// <summary>
    /// Represents a base timer class designed to be extended by more specific timer types.
    /// This class provides basic timer functionalities such as start, stop, pause, and resume.
    /// </summary>
    [Serializable]
    public abstract class Timer
    {
        [SerializeField]
        protected float initialTime; // The initial time set for the timer.

        /// <summary>
        /// The current time left on the timer.
        /// </summary>
        protected float Time { get; set; }

        /// <summary>
        /// Indicates whether the timer is currently running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Calculates and returns the progress of the timer as a percentage of the initial time. 0 ~ 1
        /// </summary>
        public float Progress => Time / initialTime;

        /// <summary>
        /// Event triggered when the timer starts.
        /// </summary>
        public Action OntimeStart = delegate { };

        /// <summary>
        /// Event triggered when the timer stops.
        /// </summary>
        public Action OnTimeStop = delegate { };

        /// <summary>
        /// Initializes a new instance of the Timer class.
        /// </summary>
        /// <param name="value">The initial time of the timer.</param>
        protected Timer(float value)
        {
            initialTime = value;
            IsRunning = false;
        }

        /// <summary>
        /// Starts or restarts the timer using the initial time.
        /// </summary>
        public void Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OntimeStart.Invoke();
            }
        }

        /// <summary>
        /// Starts or restarts the timer with a new specified time.
        /// </summary>
        /// <param name="newTime">The new time to set as the initial time.</param>
        public void Start(float newTime)
        {
            initialTime = newTime;
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OntimeStart.Invoke();
            }
        }

        /// <summary>
        /// Stops the timer and triggers the OnTimeStop event.
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimeStop.Invoke();
            }
        }

        /// <summary>
        /// Resumes the timer from its current state.
        /// </summary>
        public void Resume() => IsRunning = true;

        /// <summary>
        /// Pauses the timer, stopping the decrement of the remaining time.
        /// </summary>
        public void Pause() => IsRunning = false;

        /// <summary>
        /// An abstract method that must be implemented in derived classes to specify how the timer should update.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update call.</param>
        public abstract void Tick(float deltaTime);

        /// <summary>
        /// Gets or sets the initial duration of the timer.
        /// Used for Unity Inspector modification
        /// </summary>
        [SerializeField]
        public float InitialTime
        {
            get { return initialTime; }
            set { initialTime = value; }
        }
    }
}
