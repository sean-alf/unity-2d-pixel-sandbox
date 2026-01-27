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
}
