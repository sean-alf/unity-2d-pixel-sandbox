using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LinearAnimator : MonoBehaviour
{
    [Serializable] public class LinearAnimationDictionary : SerializableDictionary<string, LinearAnimation> { }
    [SerializeField] private bool autoStart = false;
    [SerializeField] private bool destroySelfOnFinish = false;
    [SerializeField] private LinearAnimationDictionary animations;

    [Space]
    [Header("Debug")]

    [SerializeField] private string currentAnimationKey;
    [SerializeField] private bool animateReverse = false;
    [SerializeField] private bool useUnscaledTime = false;

    private SpriteRenderer sr;
    private Coroutine coroutine;
    private LinearAnimation currentAnimation;

    private string Tag => $"{name} ({GetType().Name})";

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (animations.Count == 0 && autoStart)
        {
            Debug.LogWarning($"{name} ({GetType().Name}): autoStart is enabled but there are no animations!");
            Debug.LogWarning($"{name} ({GetType().Name}): disabling autoStart");
            autoStart = false;
        }
    }

    private void Start()
    {
        if (autoStart)
        {
            // Get the first animation in the dictionary
            var key = animations.Keys.ToArray()[0];
            Animate(key);
        }
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
        useUnscaledTime = false;
        AnimateStart(animationName, onFinished);
    }

    public void AnimateRealtime(string animationName, Action onFinished = null)
    {
        animateReverse = false;
        useUnscaledTime = true;
        AnimateStart(animationName, onFinished);
    }

    /// <summary>
    /// Animates the animation that was running before calling Stop() on it, if there is one,
    /// otherwise no-op.
    /// <br/>
    /// onDone is called when the sprite array is animated through.
    /// If "loop" is enabled then onDone gets called everytime the animation loops.
    /// If "loop" is not enabled then onDone gets called once after animation is finished.
    /// Only works in play mode.
    /// </summary>
    /// <param name="onFinished"></param>
    public void Animate(Action onFinished = null)
    {
        // Return early here since we don't want to log an error if the current key is null
        // Which is what happens in AnimateStart().
        // Having a null currentAnimationkey is a valid state.
        if (!IsKeyValid(currentAnimationKey)) return;
        animateReverse = false;
        useUnscaledTime = false;
        AnimateStart(currentAnimationKey, onFinished);
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
        useUnscaledTime = false;
        AnimateStart(animationName, onFinished);
    }

    private void AnimateStart(string animationKey, Action onFinished = null)
    {
        // Ignore request to start an animation that is currently running
        if (currentAnimationKey == animationKey && coroutine != null) return;

        if (!IsKeyValid(animationKey))
        {
            Debug.LogError($"{Tag}: animation key {animationKey} is not valid!");
            return;
        }

        if (animations.Count == 0)
        {
            Debug.LogError($"{Tag}: animation count is 0!");
            return;
        }

        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        currentAnimation = animations[animationKey];
        currentAnimationKey = animationKey;
        var sprites = currentAnimation.Sprites;

        if (sprites.Count() == 0)
        {
            Debug.LogError($"{Tag}: sprite count must be greater than 0!");
            return;
        }

        if (!Application.isPlaying)
        {
            // Simply set the first sprite of the animation
            sr.sprite = sprites[0];
            return;
        }

        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(AnimateIntern(currentAnimation, onFinished));
    }

    public void Stop(bool clearSprite = false)
    {
        coroutine.WhenNotNullClass(_ =>
        {
            StopCoroutine(coroutine);
            coroutine = null;
        });

        if (clearSprite)
        {
            sr.WhenNotNull(sr => sr.sprite = null);
        }
        else
        {
            currentAnimation.WhenNotNullClass(
                a => sr.WhenNotNull(
                    sr => a.InactiveSprite.WhenNotNull(
                        s => sr.sprite = s
                    )
                )
            );
        }
    }

    public void UpdateDuration(string key, float duration)
    {
        if (IsKeyValid(key))
        {
            animations[key].UpdateDuration(duration);
        }
        else
        {
            Debug.LogError($"{Tag}: attempting to update duration with invalid key {key}!");
        }
    }

    /// <summary>
    /// The key is valid if it is not null, non-zero length, and exists withing the animations dictionary.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsKeyValid(string key) => key != null && key.Trim().Length > 0 && animations.ContainsKey(key);

    public void ClearCurrentAnimation()
    {
        currentAnimation = null;
        currentAnimationKey = null;
    }

    private IEnumerator AnimateIntern(LinearAnimation animation, Action onFinished)
    {
        if (animation.Loop)
        {
            while (true)
            {
                yield return AnimateThrough(animation);
                onFinished?.Invoke();
                if (!animation.Loop) break;
            }
        }
        else
        {
            yield return AnimateThrough(animation);
            onFinished?.Invoke();
        }

        coroutine = null;

        if (animation.ClearSpriteOnCompletion) sr.sprite = null;
        if (destroySelfOnFinish) Destroy(gameObject);
    }

    private IEnumerator AnimateThrough(LinearAnimation animation)
    {
        var sprites = animation.Sprites;

        if (animateReverse)
        {
            for (int i = sprites.Count() - 1; i >= 0; i--)
            {
                sr.sprite = sprites[i];
                yield return useUnscaledTime ? animation.GetStepWaitRealtime() : animation.GetStepWait();
            }
        }
        else
        {
            for (int i = 0; i < sprites.Count(); i++)
            {
                sr.sprite = sprites[i];
                yield return useUnscaledTime ? animation.GetStepWaitRealtime() : animation.GetStepWait();
            }
        }
    }

    [Serializable]
    public class LinearAnimation
    {
        [SerializeField] private Sprite[] sprites;
        [SerializeField][Tooltip("The sprite to use when not animating")] private Sprite inactiveSprite;
        [SerializeField] private float duration;
        [SerializeField] private bool clearSpriteOnCompletion = false;

        [SerializeField]
        private bool loop = false;

        private WaitForSeconds stepWait;
        private WaitForSecondsRealtime stepWaitRealtime;

        public Sprite[] Sprites => sprites;
        public Sprite InactiveSprite => inactiveSprite;
        public bool Loop => loop;
        public bool ClearSpriteOnCompletion => clearSpriteOnCompletion;

        public WaitForSeconds GetStepWait()
        {
            stepWait ??= new(duration / sprites.Length);
            return stepWait;
        }

        public WaitForSecondsRealtime GetStepWaitRealtime()
        {
            stepWaitRealtime ??= new(duration / sprites.Length);
            return stepWaitRealtime;
        }

        public void UpdateDuration(float duration)
        {
            this.duration = duration;
            stepWait = new(duration / sprites.Length);
            stepWaitRealtime = new(duration / sprites.Length);
        }
    }
}
