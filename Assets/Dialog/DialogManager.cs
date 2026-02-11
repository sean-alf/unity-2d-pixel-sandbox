using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DialogManager : MonoBehaviour
{
    public bool IsShowing => gameObject.activeSelf;

    [SerializeField] private float fadeAnimationDuration = 0.25f;
    [SerializeField] private int fadeAnimationStepCount = 10;
    [SerializeField] private float charsPerSecond = 60f;
    [SerializeField] private float pageTurnDelay = 0.3f;
    [SerializeField] private float pageScrollDuration = 1f;

    [Space]
    [Header("Debug")]

    [SerializeField] private bool ignoreInput = false;

    private Image container;
    private TextMeshProUGUI text;
    private WaitForSeconds charTypeDelayWait;
    private WaitForSeconds pageTurnDelayWait;

    private void Awake()
    {
        container = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        SetAlpha(0);
        text.text = null;
        gameObject.SetActive(false);
        charTypeDelayWait = new(1f / charsPerSecond);
        pageTurnDelayWait = new(pageTurnDelay);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        charTypeDelayWait = new(1f / charsPerSecond);
        pageTurnDelayWait = new(pageTurnDelay);
    }
#endif

    public void ShowMessage(string message)
    {
        if (ignoreInput) return;

        ignoreInput = true;

        text.text = message;
        text.pageToDisplay = 1;
        text.maxVisibleCharacters = 0;

        SetAlpha(0);
        gameObject.SetActive(true);
        Fade(endAlphaValue: 1f, onDone: () => TypeMessage());
    }

    public void ShowNextMessagePageOrClose(Action onClosed)
    {
        if (ignoreInput) return;

        ignoreInput = true;

        if (text.pageToDisplay == text.textInfo.pageCount)
        {
            Close(onClosed);
        }
        else
        {
            StartCoroutine(StartTextScroll(
                onScrollToBottom: () =>
                {
                    text.maxVisibleCharacters = 0;
                },
                onDone: () =>
                {
                    text.pageToDisplay += 1;
                    TypeMessage();
                }
            ));
        }
    }

    private void Close(Action onClosed)
    {
        if (!gameObject.activeSelf) return;
        Fade(endAlphaValue: 0f, onDone: () =>
        {
            gameObject.SetActive(false);
            ignoreInput = false;
            onClosed();
        });
    }

    private void TypeMessage()
    {
        text.ForceMeshUpdate();
        var pageInfo = text.textInfo.pageInfo[text.pageToDisplay - 1];
        StartCoroutine(TypeMessageCoroutine(
            charStartIndex: pageInfo.firstCharacterIndex,
            charEndIndex: pageInfo.lastCharacterIndex
        ));
    }

    private IEnumerator TypeMessageCoroutine(int charStartIndex, int charEndIndex)
    {
        int charCount = charStartIndex + 1;
        while (charCount <= (charEndIndex + 1))
        {
            text.maxVisibleCharacters = charCount++;
            yield return charTypeDelayWait;
        }

        yield return pageTurnDelayWait;

        ignoreInput = false;
    }

    private IEnumerator StartTextScroll(Action onScrollToBottom, Action onDone)
    {
        float elapsed = 0f;
        Vector2 startPos = text.rectTransform.anchoredPosition;
        float scrollDistance = text.rectTransform.rect.height;

        while (elapsed < pageScrollDuration)
        {
            elapsed += Time.deltaTime;
            text.rectTransform.anchoredPosition = Vector2.Lerp(startPos, startPos.AddY(scrollDistance), elapsed / pageScrollDuration);
            yield return null;
        }

        onScrollToBottom();

        // Snap back to starting position
        text.rectTransform.anchoredPosition = startPos;

        onDone();
    }

    private void Fade(float endAlphaValue, Action onDone = null) => this.AnimateFloat(
        start: container.color.a,
        end: endAlphaValue,
        stepCount: fadeAnimationStepCount,
        totalDuration: fadeAnimationDuration,
        onStep: (value) => SetAlpha(value),
        onDone: onDone
    );

    private void SetAlpha(float value)
    {
        container.color = container.color.WithAlpha(value);
        text.color = text.color.WithAlpha(value);
    }
}
