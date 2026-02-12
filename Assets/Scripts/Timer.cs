using System;
using System.Collections;
using UnityEngine;

public static class Timer
{
    public static Coroutine StartTimer(this MonoBehaviour m, float duration, Action onExpired)
    {
        if (duration <= 0)
        {
            onExpired();
            return null;
        }

        IEnumerator StartTimer()
        {
            var timerCounter = duration;

            while (timerCounter > 0)
            {
                timerCounter -= Time.deltaTime;
                yield return null;
            }

            onExpired();
        }

        return m.StartCoroutine(StartTimer());
    }
}
