using System;
using System.Collections;
using UnityEngine;

public static class Animations
{
    public static Coroutine AnimateFloat(this MonoBehaviour m, float start, float end, int stepCount, float totalDuration, Action<float> onStep, Action onDone = null)
    {
        IEnumerator Animate()
        {
            if (stepCount <= 0 || totalDuration <= 0)
            {
                Debug.LogError("AnimateFloat: stepCount and totalDuration must be greater than 0");
                yield break;
            }

            float range = end - start;
            float stepDuration = totalDuration / stepCount;
            float stepValue = range / stepCount;
            WaitForSeconds wait = new(stepDuration);
            Func<float, float, bool> eval = stepValue > 0 ? (a, b) => a < b : (a, b) => a > b;
            Func<float, float, float> correct = stepValue > 0 ? (a, b) => Mathf.Min(a, b) : (a, b) => Mathf.Max(a, b);

            if (range == 0)
            {
                Debug.LogError("AnimateFloat: end - start must be non-zero");
                yield break;
            }

            if (stepDuration <= 0)
            {
                Debug.LogError("AnimateFloat: stepDuration must be greater than 0");
                yield break;
            }

            if (stepValue == 0)
            {
                Debug.LogError("AnimateFloat: stepValue must be non-zero");
                yield break;
            }

            while (eval(start, end))
            {
                start = correct(start + stepValue, end);
                onStep?.Invoke(start);
                yield return wait;
            }

            onDone?.Invoke();
        }

        return m.StartCoroutine(Animate());
    }
}
