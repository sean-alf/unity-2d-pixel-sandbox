using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LinearAnimator : MonoBehaviour
{
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
        public float Duration => duration;
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

    [Serializable]
    public class LinearAnimationDictionary : SerializableDictionary<string, LinearAnimation> { }

    [SerializeField]
    private LinearAnimationDictionary animations;

    private SpriteRenderer sr;
    private LinearAnimation currentAnimation;
    private bool animateReverse = false;

    private float timer;
    private int currentFrame;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (currentAnimation == null) return;

        timer += Time.deltaTime;
        float frameDuration = currentAnimation.Duration / currentAnimation.Sprites.Length;
        if (timer >= frameDuration)
        {
            if (animateReverse)
            {
                currentFrame = (currentAnimation.Sprites.Length + currentFrame - 1) % currentAnimation.Sprites.Length;
            }
            else
            {
                currentFrame = (currentFrame + 1) % currentAnimation.Sprites.Length;
            }

            sr.sprite = currentAnimation.Sprites[currentFrame];
            timer = 0;
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
        currentAnimation = animations[animationName];
        // AnimateStart(animationName, onFinished);
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
        currentAnimation = animations[animationName];
        // AnimateStart(animationName, onFinished);
    }

    public void Stop()
    {
        currentAnimation.WhenNotNullClass(
            a => sr.WhenNotNull(
                sr => a.InactiveSprite.WhenNotNull(
                    s => sr.sprite = s
                )
            )
        );

        currentAnimation = null;
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
}
