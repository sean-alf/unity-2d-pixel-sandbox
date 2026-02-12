using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class Animations
{
    public static Coroutine FadeIn(this MonoBehaviour m, SpriteRenderer sr, float totalDuration, Action onDone = null)
    {
        if (sr.color.a == 1f)
        {
            onDone?.Invoke();
            return null;
        }

        return m.AnimateFloat(
            start: sr.color.a,
            end: 1f,
            (1f - sr.color.a) * totalDuration,
            onStep: value => sr.color = sr.color.WithAlpha(value),
            onDone
        );
    }

    public static Coroutine FadeOut(this MonoBehaviour m, SpriteRenderer sr, float totalDuration, Action onDone = null)
    {
        if (sr.color.a == 0f)
        {
            onDone?.Invoke();
            return null;
        }

        return m.AnimateFloat(
            start: sr.color.a,
            end: 0f,
            sr.color.a * totalDuration,
            onStep: value => sr.color = sr.color.WithAlpha(value),
            onDone
        );
    }

    public static Coroutine FadeIn(this MonoBehaviour m, Image image, float totalDuration, Action onDone = null)
    {
        if (image.color.a == 1f)
        {
            onDone?.Invoke();
            return null;
        }

        return m.AnimateFloat(
            start: image.color.a,
            end: 1f,
            (1f - image.color.a) * totalDuration,
            onStep: value => image.color = image.color.WithAlpha(value),
            onDone
        );
    }

    public static Coroutine FadeOut(this MonoBehaviour m, Image image, float totalDuration, Action onDone = null)
    {
        if (image.color.a == 0f)
        {
            onDone?.Invoke();
            return null;
        }

        return m.AnimateFloat(
            start: image.color.a,
            end: 0f,
            image.color.a * totalDuration,
            onStep: value => image.color = image.color.WithAlpha(value),
            onDone
        );
    }

    public static Coroutine AnimateFloat(this MonoBehaviour m, float start, float end, float totalDuration, Action<float> onStep, Action onDone = null)
    {
        IEnumerator Animate()
        {
            if (totalDuration <= 0)
            {
                Debug.LogError("AnimateFloat: totalDuration must be greater than 0");
                yield break;
            }

            // Start and end are the same so we're done already
            if (Mathf.Abs(end - start) == 0f) onDone?.Invoke();

            float t = 0;

            while (t < 1f)
            {
                t += Time.deltaTime / totalDuration;
                t = Mathf.Clamp01(t);
                var newValue = Mathf.Lerp(start, end, t);
                onStep?.Invoke(newValue);
                yield return null;
            }

            onStep?.Invoke(end);
            onDone?.Invoke();
        }

        return m.StartCoroutine(Animate());
    }
}
