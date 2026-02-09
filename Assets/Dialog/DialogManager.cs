using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DialogManager : MonoBehaviour
{
    [SerializeField] private float fadeAnimationDuration = 0.25f;
    [SerializeField] private int fadeAnimationStepCount = 10;
    [SerializeField] private float characterTypeDuration = 0.1f;

    [Space]
    [Header("Debug")]

    [SerializeField] private List<string> messageList = new();

    private Image container;
    private TextMeshProUGUI text;
    private WaitForSeconds charTypeWait;
    private bool canBeClosed = false;
    private int lineCount = 0;


    private void Awake()
    {
        container = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        SetAlpha(0);
        text.text = null;
        gameObject.SetActive(false);
        charTypeWait = new(characterTypeDuration);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        charTypeWait = new(characterTypeDuration);
    }
#endif

    public void ShowMessage(string message)
    {
        canBeClosed = false;

        if (gameObject.activeSelf)
        {
            Debug.LogError($"DialogManager: game object is active and ShowMessage was called!");
            return;
        }

        PrepareMessage(message);
        SetAlpha(0);
        gameObject.SetActive(true);
        Fade(endAlphaValue: 1f, onDone: TypeMessage);
    }

    public void Hide(Action onDone = null)
    {
        if (!gameObject.activeSelf || !canBeClosed) return;
        Fade(endAlphaValue: 0f, onDone: () =>
        {
            gameObject.SetActive(false);
            onDone?.Invoke();
        });
    }

    private void PrepareMessage(string message)
    {
        messageList.Clear();

        var info = text.GetTextInfo(message);
        lineCount = info.lineCount;
        Debug.Log($"DialogManager: line count {lineCount}");

        foreach (var lineInfo in info.lineInfo)
        {
            if (lineInfo.characterCount == 0) continue;

            var firstIndex = lineInfo.firstVisibleCharacterIndex;

            Debug.Log($"DialogManager: line height {lineInfo.lineHeight}, first index {firstIndex}, char count {lineInfo.characterCount}");

            messageList.Add(message.Substring(firstIndex, lineInfo.characterCount).Trim());
        }

        text.text = null;
    }

    private void TypeMessage() => StartCoroutine(TypeMessageCoroutine(onDone: () => canBeClosed = true));

    private IEnumerator TypeMessageCoroutine(Action onDone)
    {
        foreach (var line in messageList)
        {
            foreach (var c in line)
            {
                text.text += c;
                yield return charTypeWait;
            }

            if (messageList.IndexOf(line) != lineCount - 1) text.text += "\n";
        }

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
