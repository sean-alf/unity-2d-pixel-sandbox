using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public static class Animations
{
    public static IEnumerator FadeIn(this SpriteRenderer sr, float duration, float steps, Action onDone)
    {
        Color c = sr.color;
        WaitForSeconds wait = new(duration / steps);
        float stepAmount = (1.0f - sr.color.a) / steps;

        while (sr.color.a < 1.0f)
        {
            c.a += stepAmount;

            if (c.a > 1.0f)
            {
                c.a = 1.0f;
            }

            sr.color = c;
            yield return wait;
        }

        onDone?.Invoke();
    }

    public static IEnumerator FadeOut(this SpriteRenderer sr, float duration, float steps, Action onDone)
    {
        Color c = sr.color;
        WaitForSeconds wait = new(duration / steps);
        float stepAmount = sr.color.a / steps;

        while (sr.color.a > 0.0f)
        {
            c.a -= stepAmount;

            if (c.a < 0.0f)
            {
                c.a = 0.0f;
            }

            sr.color = c;
            yield return wait;
        }

        onDone?.Invoke();
    }
}
