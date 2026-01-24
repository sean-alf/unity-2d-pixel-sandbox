using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(Collider2D))]
public class BlowTorchFlame : MonoBehaviour
{
    private static readonly string AnimationKeyStart = "Start";
    private static readonly string AnimationKeyLoop = "Loop";

    [SerializeField] private float runTime = 3f;
    [SerializeField] private float idleTime = 3f;
    [SerializeField] private float initialDelay = 0f;
    [SerializeField][Range(1, 50)] private int maxDistance = 1;
    [SerializeField][Range(0.1f, 2f)] private float extendRectractStep = 0.5f;
    [SerializeField][Range(0.01f, 1f)] private float extendRectractStepDuration = 0.1f;

    private SpriteRenderer sr;
    private LinearAnimator animator;
    private new Collider2D collider;
    private WaitForSeconds runTimeWait;
    private WaitForSeconds idleTimeWait;
    private WaitForSeconds initialDelayWait;
    private WaitForSeconds extendRetractWait;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<LinearAnimator>();
        collider = GetComponent<Collider2D>();

        // Start with the simple draw mode
        // And without a sprite
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sprite = null;
        collider.enabled = false;
        UpdateWaits();
    }

    private void Start()
    {
        StartCoroutine(Run());
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateWaits();
    }
#endif

    private void UpdateWaits()
    {
        runTimeWait = new(runTime);
        idleTimeWait = new(idleTime);
        initialDelayWait = new(initialDelay);
        extendRetractWait = new(extendRectractStepDuration);
    }

    private IEnumerator Run()
    {
        bool startAnimationDone = false;
        bool reverseAnimationDone = false;

        yield return initialDelayWait;
        while (true)
        {
            startAnimationDone = false;
            reverseAnimationDone = false;

            animator.Animate(AnimationKeyStart, () =>
            {
                collider.enabled = true;
                startAnimationDone = true;
            });
            yield return new WaitUntil(() => startAnimationDone);

            if (maxDistance > 1)
            {
                sr.drawMode = SpriteDrawMode.Tiled;

                while (sr.size.y < maxDistance)
                {
                    sr.size = sr.size.AddY(extendRectractStep);
                    if (sr.size.y >= maxDistance) break;
                    yield return extendRetractWait;
                }
            }

            animator.Animate(AnimationKeyLoop);

            yield return runTimeWait;

            animator.Stop();

            if (sr.drawMode == SpriteDrawMode.Tiled)
            {
                while (sr.size.y > 1)
                {
                    sr.size = sr.size.SubtractY(extendRectractStep);
                    if (sr.size.y <= 1) break;
                    yield return extendRetractWait;
                }
                sr.drawMode = SpriteDrawMode.Simple;
            }

            collider.enabled = false;

            animator.AnimateReverse(AnimationKeyStart, () =>
            {
                sr.WhenNotNull(sr => sr.sprite = null);
                reverseAnimationDone = true;
            });
            yield return new WaitUntil(() => reverseAnimationDone);
            yield return idleTimeWait;
        }
    }
}
