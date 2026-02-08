using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LinearAnimator))]
public class Teleport : MonoBehaviour,
    ILoggerProvider,
    ISceneTransitionPoint,
    BetterInputManager.IInputChangeRequestor
{
    public enum TravelType
    {
        Bidirectional, // Can work as an entry and exit teleport
        Oneway, // Like the entry teleport on the first level    
    }

    [SerializeField][Range(0, 100)] private int inputMapSwitchingPriority = 80;
    [SerializeField] private GameObject teleportingAnimationTemplate;
    [SerializeField] private TransitionType transitionType;

    // Exposed for ST2U prefab replacer
    public TravelType travelType;

    [SerializeField][Range(32, 128)] private int distanceToReactivatePX = 32;
    [SerializeField] private int pixelsPerUnit = 32;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private int fadeSteps = 6;
    [SerializeField] private bool isTeleporting = false;

    [Space]
    [Header("Debug")]

    [SerializeField] private Logger logger;

    private SpriteRenderer sr;
    private LinearAnimator animator;

    private static readonly Vector2 FinishingDirection = Vector2.up;

    public Action<TransitionType> OnExit { get; set; }

    public TransitionType TransitionType => transitionType;
    public int Priority => inputMapSwitchingPriority;
    public string Name => $"{name} ({GetType().Name})";
    public BetterInputManager.InputType InputType => BetterInputManager.InputType.None;
    public Logger Logger => logger;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<LinearAnimator>();

        Debug.Log($"{name} ({GetType().Name}): Awake: travelType {travelType}");
        switch (travelType)
        {
            case TravelType.Oneway:
                {
                    Activate(false);
                    break;
                }
            case TravelType.Bidirectional:
                {
                    Activate(true);
                    break;
                }
        }
    }

    private void Start()
    {
        animator.Animate("Default");
    }

    public void AnimateAndTeleportToNextScene(GameObject target)
    {
        if (isTeleporting) return;

        isTeleporting = true;

        var inputManager = target.GetComponent<BetterInputManager>();
        var autoMover = target.GetComponent<AutoMover>();

        inputManager.AddInputChangeRequest(this);
        autoMover.MoveTo(transform.position, FinishingDirection, () =>
        {
            // OnDone
            RunTeleportationAnimation(inputManager, false);
        });
    }

    /// <summary>
    /// Gets called before Enter() within SceneSwitcher regardless of whether all 
    /// scripts have called Start().
    /// </summary>
    public void PrepareToEnter()
    {
        Activate(false);
        sr.color = sr.color.WithAlpha(1f);
        animator.Animate("Default");

        // Let's not assume that we know if the PlayerController will be active at this point
        var target = FindAnyObjectByType<BetterInputManager>(FindObjectsInactive.Include);
        var visibilityManager = target.gameObject.GetComponent<SpriteRendererVisibilityManager>();
        var playerController = target.GetComponent<PlayerController>();

        target.AddInputChangeRequest(this);
        playerController.UpdateDirection(FinishingDirection);
        visibilityManager.SetVisibility(visible: false);
        target.transform.position = transform.position;
    }

    /// <summary>
    /// Gets called after PrepareToEnter(), after all other scripts have called Start(), 
    /// and after a predefined delay set on the Synchronizer component.
    /// </summary>
    public void Enter()
    {
        // SceneManager may try to acces this Teleport when multiple scenes are loaded in the Scene Editor
        // The Teleport may be null by that point due to unloading of non-target scenes
        if (this == null) return;

        var target = FindAnyObjectByType<BetterInputManager>(FindObjectsInactive.Include);

        if (target != null)
        {
            RunTeleportationAnimation(target, true);
        }
        else
        {
            logger.E($"{nameof(PlayerController)} not found!!");
        }
    }

    private void RunTeleportationAnimation(BetterInputManager target, bool teleportIn)
    {
        var template = Instantiate(teleportingAnimationTemplate);
        var teleportingAnimator = template.GetComponent<TeleportingAnimator>();

        template.transform.position = transform.position;
        teleportingAnimator.Animate(
            onCover: () => target.gameObject.GetComponent<SpriteRendererVisibilityManager>().SetVisibility(visible: teleportIn),
            onDone: () =>
            {
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

                            target.RemoveInputChangeRequest(this);
                        }
                    );
                }
                else
                {
                    // This must be done so that there is only one PlayerInput instance in the scene at a time
                    // Otherwise it gets cleard out
                    target.gameObject.SetActive(false);
                    Activate(false);
                    OnExit?.Invoke(transitionType);
                }

                isTeleporting = false;
            }
        );
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
