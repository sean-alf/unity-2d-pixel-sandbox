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

    [Serializable]
    public class LinearAnimation
    {
        public Sprite[] sprites;

        [Tooltip("The sprite to use when not animating")]
        public Sprite inactiveSprite;

        public DelayType type;

        public float delayDuration;

        public bool loop = false;
    }

    [Serializable]
    public class LinearAnimationDictionary : SerializableDictionary<string, LinearAnimation> { }

    [SerializeField]
    private LinearAnimationDictionary animations;

    private SpriteRenderer sr;
    private Coroutine coroutine;
    private LinearAnimation currentAnimation;
    private WaitForSeconds waitForStepDelayDuration;
    private bool animateReverse = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// onDone is called when the sprite array is animated through.
    /// If "loop" is enabled then onDone gets called everytime the animation loops.
    /// If "loop" is not enabled then onDone gets called once after animation is finished.
    /// Only works in play mode.
    /// </summary>
    /// <param name="onFinished"></param>
    public void Animate(string animationName, Action onFinished = null)
    {
        animateReverse = false;
        AnimateStart(animationName, onFinished);
    }

    /// <summary>
    /// onDone is called when the sprite array is animated through.
    /// If "loop" is enabled then onDone gets called everytime the animation loops.
    /// If "loop" is not enabled then onDone gets called once after animation is finished.
    /// Only works in play mode.
    /// </summary>
    /// <param name="onFinished"></param>
    public void AnimateReverse(string animationName, Action onFinished = null)
    {
        animateReverse = true;
        AnimateStart(animationName, onFinished);
    }

    private void AnimateStart(string animationName, Action onFinished = null)
    {
        if (coroutine != null) return;

        if (animations.Count == 0)
        {
            Debug.LogError($"LinearAnimator ({gameObject.name}): animation count is 0!");
        }

        if (!animations.ContainsKey(animationName))
        {
            Debug.LogError($"LinearAnimator ({gameObject.name}): invalid animation name {animationName}!");
            return;
        }

        currentAnimation = animations[animationName];
        var sprites = currentAnimation.sprites;

        SetStepDelay(currentAnimation);

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

        coroutine = StartCoroutine(AnimateIntern(currentAnimation, onFinished));
    }

    public void Stop()
    {
        coroutine.WhenNotNullClass(_ =>
        {
            StopCoroutine(coroutine);
            coroutine = null;
        });

        currentAnimation.WhenNotNullClass(
            a => sr.WhenNotNull(
                sr => a.inactiveSprite.WhenNotNull(
                    s => sr.sprite = s
                )
            )
        );
    }

    private IEnumerator AnimateIntern(LinearAnimation animation, Action onFinished)
    {
        if (animation.loop)
        {
            while (true)
            {
                yield return AnimateThrough(animation);
                onFinished?.Invoke();
            }
        }
        else
        {
            yield return AnimateThrough(animation);
            coroutine = null;
            onFinished?.Invoke();
        }
    }

    private IEnumerator AnimateThrough(LinearAnimation animation)
    {
        var sprites = animation.sprites;

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
        if (currentAnimation == null) return;
        SetStepDelay(currentAnimation);
    }

    private void SetStepDelay(LinearAnimation animation)
    {
        float stepDelay = animation.delayDuration;

        if (animation.type == DelayType.TOTAL) stepDelay = animation.delayDuration / animation.sprites.Count();

        waitForStepDelayDuration = new(stepDelay);
    }
}
