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

    [SerializeField][Range(1, 4)][Tooltip("Distance in world units")] private int distanceToReactivate = 2;
    [SerializeField] private float fadeDuration = 0.5f;

    [Space]
    [Header("Debug")]

    [SerializeField] private bool isTeleporting = false;
    [SerializeField] private bool isEntering = false;
    [SerializeField] private Logger logger;

    private SpriteRenderer sr;
    private LinearAnimator animator;

    private static readonly Vector2 FinishingDirection = Vector2.up;

    public Action<TransitionType> OnExit { get; set; }

    public TransitionType TransitionType => transitionType;
    public int Priority => inputMapSwitchingPriority;
    public string Name => Tag;
    public BetterInputManager.InputType InputType => BetterInputManager.InputType.None;
    public Logger Logger => logger;

    private string Tag => $"{name} ({GetType().Name})";

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<LinearAnimator>();

        // Debug.Log($"{Tag}: Awake: travelType {travelType}");

        var isBidirectional = travelType == TravelType.Bidirectional;
        Activate(isBidirectional);
        sr.color = sr.color.WithAlpha(isBidirectional ? 1f : 0f);
        if (isBidirectional) animator.Animate("Default");
    }

    // Start gets called AFTER PrepareToEnter
    private void Start()
    {
        if (isEntering) return;
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
        // Debug.Log($"{Tag}: PrepareToEnter");

        isEntering = true;
        animator.Animate("Default");
        sr.color = sr.color.WithAlpha(1f);
        Activate(false);

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
                    this.FadeOut(sr,
                        totalDuration: fadeDuration,
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
                    // Otherwise it gets cleared out
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
        while (Vector2.Distance(target.transform.position, transform.position) < distanceToReactivate)
        {
            yield return null;
        }

        this.FadeIn(sr,
            totalDuration: fadeDuration,
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
