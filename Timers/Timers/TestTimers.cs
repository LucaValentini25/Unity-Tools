using UnityEngine;

namespace Utilities.Timers
{
    public class TestTimers : MonoBehaviour
    {
        // StopWatchTimer instance used to measure elapsed time.
        private StopWatchTimer _stopwatchTimer;

        // CountdownTimer instance used for countdown functionality.
        // Serialized to allow timer initialization settings via Unity Inspector.
        [SerializeField] private CountdownTimer countdownTimer;

        void Start()
        {
            // Initialize the stopwatch and start it immediately.
            _stopwatchTimer = new StopWatchTimer();
            _stopwatchTimer.Start();

            // Subscribe to start and stop events for logging.
            _stopwatchTimer.OntimeStart += () => Debug.Log("StopWatch Start!");
            _stopwatchTimer.OnTimeStop += () => Debug.Log("StopWatch Stop!");
        
            // Initialize the countdown timer using the inspector-set initial time, if available.
            // Pass 'false' to prevent it from stopping when the countdown finishes.
            countdownTimer = new CountdownTimer(countdownTimer.InitialTime, false);

            // Subscribe to the start, end, and stop events for logging.
            countdownTimer.OntimeStart += () => Debug.Log("Countdown Start!");
            countdownTimer.OnTimeEnd += () => Debug.Log("Countdown Finished!");
            countdownTimer.OnTimeStop += () => Debug.Log("Countdown Stop!");

            // Start the countdown timer.
            countdownTimer.Start();
        }

        void Update()
        {
            // Update both timers with the delta time to advance their internal clocks.
            _stopwatchTimer.Tick(Time.deltaTime);
            countdownTimer.Tick(Time.deltaTime);

            // Log and manage stopwatch based on its time.
            if (_stopwatchTimer.GetTime() > 5)
            {
                Debug.Log("StopWatch is more than 5 seconds");
                Debug.Log($"Stopwatch Time: {_stopwatchTimer.GetTime()} seconds");

                // Stop, pause, and resume stopwatch to demonstrate these controls.
                _stopwatchTimer.Stop();
                _stopwatchTimer.Pause();
                _stopwatchTimer.Resume();
            }

            // Log countdown progress and demonstrate timer controls.
            Debug.Log($"Countdown Remaining: {countdownTimer.Progress * 100}% progress");
            countdownTimer.Stop();
            countdownTimer.Pause();
            countdownTimer.Resume();
            countdownTimer.Reset();
        }
    }
}
