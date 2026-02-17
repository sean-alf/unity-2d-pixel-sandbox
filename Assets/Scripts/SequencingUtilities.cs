using System;
using System.Collections;
using UnityEngine;

public static class SequencingUtilities
{
    public static IEnumerator Delay(float duration, Action onRun)
    {
        yield return new WaitForSeconds(duration);
        onRun();
    }

    public static IEnumerator DelayRealtime(float duration, Action onRun)
    {
        yield return new WaitForSecondsRealtime(duration);
        onRun();
    }

    public static IEnumerator WaitForSecondsWhile(float duration, Func<bool> whileCondition)
    {
        float timer = 0f;

        while (timer < duration && whileCondition())
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
