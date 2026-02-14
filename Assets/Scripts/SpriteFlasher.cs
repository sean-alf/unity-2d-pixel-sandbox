using System.Collections;
using UnityEngine;

public class SpriteFlasher : MonoBehaviour
{
    [ColorUsage(true, true)]  // ← this enables HDR mode in Inspector!
    [SerializeField] private Color flashColor = new(2f, 2f, 2f, 1f);
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] private float stepDuration = 0.25f;
    [SerializeField] private bool demoColor = false;
    [SerializeField] private Color originalColor = Color.white;

    private Coroutine coroutine;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spriteRenderers == null) return;

        foreach (var sr in spriteRenderers)
        {
            if (sr == null) continue;
            sr.color = demoColor ? flashColor : originalColor;
        }
    }
#endif

    public void StartFlash()
    {
        if (coroutine != null) return;
        coroutine = StartCoroutine(FlashCoroutine());
    }

    public void StopFlash()
    {
        if (coroutine == null) return;
        StopCoroutine(coroutine);
        foreach (var sr in spriteRenderers) sr.color = originalColor;
        coroutine = null;
    }

    public IEnumerator FlashCoroutine()
    {
        bool useFlashColor = true;

        while (true)
        {
            foreach (var sr in spriteRenderers) sr.color = useFlashColor ? flashColor : originalColor;
            useFlashColor = !useFlashColor;
            yield return new WaitForSeconds(stepDuration);
        }
    }
}
