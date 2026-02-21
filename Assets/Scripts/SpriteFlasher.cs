using System;
using System.Collections;
using UnityEngine;

public class SpriteFlasher : MonoBehaviour
{
    public enum ColorToShow
    {
        Original,
        FlashColor1,
        FlashColor2,
    }

    [SerializeField][ColorUsage(true, true)] private Color flashColor = new(2f, 2f, 2f, 1f);
    [SerializeField][ColorUsage(true, true)] private Color flashColor2 = new(2f, 2f, 2f, 1f);
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] private float stepDuration = 0.05f;
    [SerializeField] private ColorToShow colorToShow;
    [SerializeField] private Color originalColor = Color.white;

    private Coroutine coroutine;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spriteRenderers == null) return;

        foreach (var sr in spriteRenderers)
        {
            if (sr == null) continue;
            switch (colorToShow)
            {
                case ColorToShow.Original:
                    sr.color = originalColor;
                    break;
                case ColorToShow.FlashColor1:
                    sr.color = flashColor;
                    break;
                case ColorToShow.FlashColor2:
                    sr.color = flashColor2;
                    break;
            }
        }
    }
#endif

    public void StartFlash(float duration = float.MaxValue, Action onDone = null)
    {
        if (coroutine != null || this == null) return;
        coroutine = StartCoroutine(FlashCoroutine(duration, onDone));
    }

    public void StopFlash()
    {
        if (coroutine == null) return;
        StopCoroutine(coroutine);
        foreach (var sr in spriteRenderers) sr.color = originalColor;
        coroutine = null;
    }

    private IEnumerator FlashCoroutine(float duration, Action onDone)
    {
        float totalDuration = 0;
        bool useFlashColor = true;

        while (true)
        {
            foreach (var sr in spriteRenderers) sr.color = useFlashColor ? flashColor : flashColor2;
            useFlashColor = !useFlashColor;
            yield return new WaitForSeconds(stepDuration);

            totalDuration += stepDuration;

            if (totalDuration >= duration)
            {
                break;
            }
        }

        foreach (var sr in spriteRenderers) sr.color = originalColor;
        coroutine = null;
        onDone?.Invoke();
    }
}
