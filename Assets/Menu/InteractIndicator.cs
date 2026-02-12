using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InteractIndicator : MonoBehaviour
{
    [SerializeField][Range(0.1f, 1.0f)] private float fadeDuration = 0.5f;

    private Image image;
    private Coroutine coroutine = null;

    private bool IsAnimating => coroutine != null;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.color = image.color.WithAlpha(0);
    }

    public void Listener_SetVisibility(bool show)
    {
        if (show)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        if (image.color.a == 1.0f) return;

        if (IsAnimating)
        {
            this.StopCoroutine(coroutine);
        }

        coroutine = this.AnimateFloat(
            start: image.color.a,
            end: 1.0f,
            totalDuration: fadeDuration,
            onStep: (newValue) =>
            {
                // On Step
                image.color = image.color.WithAlpha(newValue);

            },
            onDone: () =>
            {
                // On Done
                coroutine = null;
            }
        );
    }

    private void Hide()
    {
        if (image.color.a == 0.0f) return;

        if (IsAnimating)
        {
            StopCoroutine(coroutine);
        }

        coroutine = this.AnimateFloat(
            start: image.color.a,
            end: 0.0f,
            totalDuration: fadeDuration,
            onStep: (newValue) =>
            {
                image.color = image.color.WithAlpha(newValue);
            },
            onDone: () =>
            {
                coroutine = null;
            }
        );
    }
}
