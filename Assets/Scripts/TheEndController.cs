using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(RectTransform))]
public class TheEndController : MonoBehaviour
{
    [SerializeField] private float wordFadeInDuration = 1f;
    [SerializeField] private int wordFadeStepCount = 5;
    [SerializeField] private float scrollInDuration = 1f;

    private TextMeshProUGUI tmpText;
    private RectTransform rect;
    private float yPos;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();

        ResetToInitial();
    }

    public void Enter()
    {
        StopAllCoroutines();
        ResetToInitial();
        FadeIn();
        ScrollIn();
    }

    public void Hide()
    {
        StopAllCoroutines();
        ResetToInitial();
    }

    private void FadeIn() => StartCoroutine(FadeInCoroutine());

    private void ScrollIn() => StartCoroutine(ScrollInCoroutine());

    private void ResetToInitial()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        if (rect == null) rect = GetComponent<RectTransform>();

        tmpText.color = tmpText.color.WithAlpha(0f);
        tmpText.ForceMeshUpdate();
        yPos = -180 - (tmpText.textBounds.size.y / 2);
        rect.anchoredPosition = new(rect.anchoredPosition.x, yPos);
    }

    private IEnumerator ScrollInCoroutine()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / scrollInDuration;
            var newPos = Mathf.Lerp(yPos, 0f, t);
            rect.anchoredPosition = new(rect.anchoredPosition.x, newPos);
            yield return null;
        }

        rect.anchoredPosition = new(rect.anchoredPosition.x, 0f);
    }

    private IEnumerator FadeInCoroutine()
    {
        var lineInfo = tmpText.textInfo.lineInfo[0];
        float fadeTimerCounter = wordFadeInDuration;
        float alphaStep = 1f / wordFadeStepCount;
        float alpha = 0f;
        int startIndex = lineInfo.firstVisibleCharacterIndex;
        int endIndex = lineInfo.lastVisibleCharacterIndex;

        while (true)
        {
            fadeTimerCounter -= Time.deltaTime;

            if (fadeTimerCounter <= 0)
            {
                fadeTimerCounter = wordFadeInDuration;
                alpha += alphaStep;

                if (alpha >= 1f)
                {
                    SetCharacterAlphaRange(startIndex, endIndex, 1f);
                    alpha = 0f;

                    if (startIndex == 0)
                    {
                        lineInfo = tmpText.textInfo.lineInfo[1];
                        startIndex = lineInfo.firstVisibleCharacterIndex;
                        endIndex = lineInfo.lastVisibleCharacterIndex;
                    }
                    else
                    {
                        // We're done!
                        break;
                    }
                }
                else
                {
                    SetCharacterAlphaRange(startIndex, endIndex, alpha);
                }
            }

            yield return null;
        }
    }

    private void SetCharacterAlphaRange(int startChar, int endChar, float alpha)
    {
        TMP_TextInfo textInfo = tmpText.textInfo;

        for (int i = startChar; i <= endChar && i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;

            // Get current color of first vertex (RGB preserved)
            Color32 baseColor = vertexColors[vertIndex + 0];

            // Apply new alpha only
            byte newAlpha = (byte)(alpha * 255);
            Color32 newColor = baseColor.WithAlpha(alpha);

            // Apply to all 4 vertices of the character quad
            vertexColors[vertIndex + 0] = newColor;
            vertexColors[vertIndex + 1] = newColor;
            vertexColors[vertIndex + 2] = newColor;
            vertexColors[vertIndex + 3] = newColor;
        }

        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
