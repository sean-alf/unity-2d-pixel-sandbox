using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LinearAnimator : MonoBehaviour
{
    public enum DelayType
    {
        STEP,
        TOTAL,
    }

    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    [Tooltip("The sprite to use when not animating")]
    private Sprite inactiveSprite;

    [SerializeField]
    private DelayType type;

    [SerializeField]
    private float delayDuration;

    [SerializeField]
    private bool loop = false;

    private SpriteRenderer sr;
    private Coroutine coroutine;
    private WaitForSeconds waitForStepDelayDuration;
    private bool animateReverse = false;

    private void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        SetStepDelay();
    }

    /// <summary>
    /// onDone is called when the sprite array is animated through.
    /// If "loop" is enabled then onDone gets called everytime the animation loops.
    /// If "loop" is not enabled then onDone gets called once after animation is finished.
    /// Only works in play mode.
    /// </summary>
    /// <param name="onFinished"></param>
    public void Animate(Action onFinished = null)
    {
        animateReverse = false;
        AnimateStart(onFinished);
    }

    /// <summary>
    /// onDone is called when the sprite array is animated through.
    /// If "loop" is enabled then onDone gets called everytime the animation loops.
    /// If "loop" is not enabled then onDone gets called once after animation is finished.
    /// Only works in play mode.
    /// </summary>
    /// <param name="onFinished"></param>
    public void AnimateReverse(Action onFinished = null)
    {
        animateReverse = true;
        AnimateStart(onFinished);
    }

    private void AnimateStart(Action onFinished = null)
    {
        if (sprites.Count() == 0)
        {
            Debug.LogError($"LinearAnimator ({gameObject.name}): sprite count must be greater than 0!");
            return;
        }

        if (!Application.isPlaying)
        {
            // Simply set the first sprite of the animation
            if (sr == null)
            {
                sr = GetComponent<SpriteRenderer>();
            }

            sr.sprite = sprites[0];
            return;
        }

        if (sr == null) return;

        coroutine = StartCoroutine(AnimateIntern(onFinished));
    }

    public void Stop()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        if (inactiveSprite != null) sr.sprite = inactiveSprite;
    }

    private IEnumerator AnimateIntern(Action onFinished)
    {
        if (loop)
        {
            while (true)
            {
                yield return AnimateThrough();
                onFinished?.Invoke();
            }
        }
        else
        {
            yield return AnimateThrough();
            onFinished?.Invoke();
        }
    }

    private IEnumerator AnimateThrough()
    {
        if (animateReverse)
        {
            for (int i = sprites.Count() - 1; i >= 0; i--)
            {
                sr.sprite = sprites[i];
                yield return waitForStepDelayDuration;
            }
        }
        else
        {
            for (int i = 0; i < sprites.Count(); i++)
            {
                sr.sprite = sprites[i];
                yield return waitForStepDelayDuration;
            }
        }
    }

    private void OnValidate()
    {
        SetStepDelay();
    }

    private void SetStepDelay()
    {
        float stepDelay = delayDuration;

        if (type == DelayType.TOTAL) stepDelay = delayDuration / sprites.Count();

        waitForStepDelayDuration = new(stepDelay);
    }
}
