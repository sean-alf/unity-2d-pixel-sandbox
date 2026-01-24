using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LinearAnimator : MonoBehaviour
{
    [Serializable]
    public class LinearAnimationDictionary : SerializableDictionary<string, LinearAnimation> { }

    [SerializeField]
    private LinearAnimationDictionary animations;

    private SpriteRenderer sr;
    private Coroutine coroutine;
    private LinearAnimation currentAnimation;
    private string currentAnimationKey;
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
        if (currentAnimationKey == animationName && coroutine != null) return;

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
        currentAnimationKey = animationName;
        var sprites = currentAnimation.Sprites;

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

        if (coroutine != null) StopCoroutine(coroutine);
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
                sr => a.InactiveSprite.WhenNotNull(
                    s => sr.sprite = s
                )
            )
        );
    }

    public void SetDuration(string key, float duration)
    {
        if (animations.ContainsKey(key))
        {
            animations[key].UpdateDuration(duration);
        }
        else
        {
            Debug.LogError($"LinearAnimator ({gameObject.name}): no animation with key {key}!");
        }
    }

    public bool IsKeyValid(string key) => key != null && key.Trim().Length > 0;

    private IEnumerator AnimateIntern(LinearAnimation animation, Action onFinished)
    {
        if (animation.Loop)
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
        var sprites = animation.Sprites;

        if (animateReverse)
        {
            for (int i = sprites.Count() - 1; i >= 0; i--)
            {
                sr.sprite = sprites[i];
                yield return animation.GetStepWait();
            }
        }
        else
        {
            for (int i = 0; i < sprites.Count(); i++)
            {
                sr.sprite = sprites[i];
                yield return animation.GetStepWait();
            }
        }
    }

    [Serializable]
    public class LinearAnimation
    {
        [SerializeField]
        private Sprite[] sprites;

        [SerializeField]
        [Tooltip("The sprite to use when not animating")]
        private Sprite inactiveSprite;

        [SerializeField]
        private float duration;

        [SerializeField]
        private bool loop = false;

        private WaitForSeconds stepWait;

        public Sprite[] Sprites => sprites;
        public Sprite InactiveSprite => inactiveSprite;
        public bool Loop => loop;

        public WaitForSeconds GetStepWait()
        {
            stepWait ??= new(duration / sprites.Length);
            return stepWait;
        }

        public void UpdateDuration(float duration)
        {
            this.duration = duration;
            stepWait = new(duration / sprites.Length);
        }
    }
}
