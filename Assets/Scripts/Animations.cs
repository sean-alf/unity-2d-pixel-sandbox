using System;
using System.Collections;
using UnityEngine;

public static class Animations
{
    public static Coroutine FadeIn(this MonoBehaviour m, SpriteRenderer sr, int stepCount, float totalDuration, Action onDone = null)
    {
        if (sr.color.a == 1f)
        {
            onDone?.Invoke();
            return null;
        }

        return m.AnimateFloat(
            start: sr.color.a,
            end: 1f,
            stepCount,
            totalDuration,
            onStep: value => sr.color = sr.color.WithAlpha(value),
            onDone
        );
    }

    public static Coroutine FadeOut(this MonoBehaviour m, SpriteRenderer sr, int stepCount, float totalDuration, Action onDone = null)
    {
        if (sr.color.a == 0f)
        {
            onDone?.Invoke();
            return null;
        }

        return m.AnimateFloat(
            start: sr.color.a,
            end: 0f,
            stepCount,
            totalDuration,
            onStep: value => sr.color = sr.color.WithAlpha(value),
            onDone
        );
    }

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
                // Not really an error
                // Debug.LogError("AnimateFloat: end - start must be non-zero");
                onDone?.Invoke();
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
