using System;
using UnityEngine;

/// <summary>
/// Handles timed events.
/// </summary>
public class Timer
{
    private float timerLength;
    private bool shouldRestart;

    public float TimeElapsed { get; set; }
    public float TimeRemaining
    {
        get { return timerLength - TimeElapsed; }
        set
        {
            TimeElapsed -= (value - TimeRemaining);
        }
    }

    public bool RestartWhenTimerGoesOff
    {
        get { return shouldRestart; }
        set { shouldRestart = value; if (TimeRemaining <= 0.0f) TimeElapsed = 0.0f; }
    }

    /// <summary>
    /// The type of function called when this timer goes off.
    /// </summary>
    /// <param name="deltaTimeSinceAlarm">Difference in time between the elapsed time and the length of the timer. Will always be be between 0 and the frame length.</param>
    public delegate void TimerFunc(float deltaTimeSinceAlarm);
    /// <summary>
    /// The event raised when The timer goes off.
    /// </summary>
    public event TimerFunc OnTimerWentOff;

    public Timer(float length, bool restartWhenTimerGoesOff = false, TimerFunc alarmFunction = null)
    {
        timerLength = length;
        TimeElapsed = 0.0f;
        RestartWhenTimerGoesOff = restartWhenTimerGoesOff;

        if (alarmFunction != null)
        {
            OnTimerWentOff += alarmFunction;
        }
    }

    public void Update(float elapsedTime)
    {
        if (TimeRemaining > 0.0f)
        {
            TimeElapsed += elapsedTime;

            if (TimeRemaining <= 0.0f)
            {
                OnTimerWentOff(-TimeRemaining);

                if (RestartWhenTimerGoesOff)
                {
                    TimeElapsed = 0.0f;
                }
            }
        }
    }

    public override bool Equals(object obj)
    {
        Timer t = obj as Timer;

        return t != null && t == this;
    }
    public override int GetHashCode()
    {
        return (int)(TimeRemaining);
    }
    public override string ToString()
    {
        return (RestartWhenTimerGoesOff ?
            "An endless timer going off every " + timerLength + " seconds. Time remaining: " + TimeRemaining :
            "A single-use timer going off in: " + TimeRemaining + " seconds.");
    }
}