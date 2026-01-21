using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LinearAnimator))]
public class Teleport : MonoBehaviour, ILoggerProvider, ISceneTransitionPoint
{
    public enum TravelType
    {
        Bidirectional, // Can work as an entry and exit teleport
        Oneway, // Like the entry teleport on the first level    
    }

    [SerializeField]
    private GameObject teleportingAnimationTemplate;

    [SerializeField]
    private TransitionType transitionType;

    // Exposed for ST2U prefab replacer
    public TravelType travelType;

    [SerializeField]
    [Range(32, 128)]
    private int distanceToReactivatePX = 32;

    [SerializeField]
    private int pixelsPerUnit = 32;

    [SerializeField]
    private float fadeDuration = 0.5f;

    [SerializeField]
    private int fadeSteps = 6;

    [Space]
    [Header("Debug")]

    [SerializeField]
    private Logger logger;

    private SpriteRenderer sr;
    private LinearAnimator animator;

    public Logger Logger => logger;

    public Action<TransitionType> OnExit { get; set; }

    public TransitionType TransitionType => transitionType;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<LinearAnimator>();

        switch (travelType)
        {
            case TravelType.Oneway:
                {
                    var color = sr.color;
                    color.a = 0.0f;
                    sr.color = color;
                    Activate(false);
                    break;
                }
            case TravelType.Bidirectional:
                {
                    animator.Animate("Default");
                    Activate(true);
                    break;
                }
        }
    }

    public void AnimateAndTeleportToNextScene(GameObject target)
    {
        if (target.TryGetComponent(out PlayerController p))
        {
            p.UpdateInputType(PlayerController.InputType.AutoMoving);
        }
        else
        {
            logger.E($"{target.name}: no {nameof(PlayerController)} attached!!");
        }

        if (target.TryGetComponent(out AutoMover a))
        {
            a.MoveTo(transform.position, () =>
            {
                // OnDone
                Exit(p);
            });
        }
        else
        {
            logger.E($"{target.name}: no {nameof(AutoMover)} attached!!");
        }
    }

    /// <summary>
    /// Gets called before Enter() within SceneSwitcher regardless of whether all 
    /// scripts have called Start().
    /// </summary>
    public void PrepareToEnter()
    {
        // Let's not assume that we know if the PlayerController will be active at this point
        var target = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);

        target.gameObject.SetActive(false);
        target.transform.position = transform.position;

        var color = sr.color;
        color.a = 1.0f;
        sr.color = color;
        animator.Animate("Default");
        Activate(false);
    }

    /// <summary>
    /// Gets called after PrepareToEnter(), after all other scripts have called Start(), 
    /// and after a predefined delay set on the Synchronizer component.
    /// </summary>
    public void Enter()
    {
        // The PlayerController IS inactive at this point
        var target = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);

        if (target != null)
        {
            RunTeleportationAnimation(target, true);
        }
        else
        {
            logger.E($"{nameof(PlayerController)} not found!!");
        }
    }

    private void Exit(PlayerController target)
    {
        RunTeleportationAnimation(target, false);
    }

    private void RunTeleportationAnimation(PlayerController target, bool teleportIn)
    {
        var template = Instantiate(teleportingAnimationTemplate);
        template.transform.position = transform.position;

        if (template.TryGetComponent(out TeleportingAnimator ta))
        {
            ta.Animate(() =>
            {
                // On Cover
                target.gameObject.SetActive(teleportIn);
                target.UpdateInputType(PlayerController.InputType.AutoMoving);
            }, () =>
            {
                // On Done
                if (teleportIn)
                {
                    this.AnimateFloat(
                        start: 1.0f,
                        end: 0.0f,
                        stepCount: fadeSteps,
                        totalDuration: fadeDuration,
                        onStep: newValue => sr.color = sr.color.WithAlpha(newValue),
                        onDone: () =>
                        {
                            animator.Stop();

                            if (travelType == TravelType.Bidirectional)
                            {
                                StartCoroutine(WatchPlayerDistance(target.gameObject));
                            }

                            target.UpdateInputType(PlayerController.InputType.Full);
                        }
                    );
                }
                else
                {
                    OnExit?.Invoke(transitionType);
                }
            });
        }
        else
        {
            logger.E($"template ({teleportingAnimationTemplate.name}): no {nameof(TeleportingAnimator)} attached!");
        }
    }

    private IEnumerator WatchPlayerDistance(GameObject target)
    {
        while (Vector2.Distance(target.transform.position, transform.position) < (distanceToReactivatePX / (float)pixelsPerUnit))
        {
            yield return null;
        }

        this.AnimateFloat(
            start: 0.0f,
            end: 1.0f,
            stepCount: fadeSteps,
            totalDuration: fadeDuration,
            onStep: newValue => sr.color = sr.color.WithAlpha(newValue),
            onDone: () =>
            {
                animator.Animate("Default");
                Activate(true);
            }
        );
    }

    private void Activate(bool activate)
    {
        if (TryGetComponent(out Collider2D collider2D)) collider2D.enabled = activate;
    }
}
