using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class WashoutController : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private float totalFadeDuration = 0.25f;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.color = image.color.WithAlpha(0f);
    }

    private void OnEnable()
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasRectTransform.rect.width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasRectTransform.rect.height);
    }

    public void Show() => image.color = image.color.WithAlpha(1f);

    public void Hide() => image.color = image.color.WithAlpha(0f);

    public void FadeIn(Action onDone = null) => this.FadeIn(image, totalFadeDuration, onDone);

    public void FadeOut(Action onDone = null) => this.FadeOut(image, totalFadeDuration, onDone);
}
