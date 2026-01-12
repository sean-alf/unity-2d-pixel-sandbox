using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Teleport : MonoBehaviour, ILoggerProvider, ISceneTransitionPoint
{
    public enum TravelType
    {
        Bidirectional, // Can work as an entry and exit teleport
        Oneway, // Like the entry teleport on the first level    
    }

    [SerializeField]
    private bool activated = false;

    [SerializeField]
    private GameObject teleportingAnimationTemplate;

    [SerializeField]
    private TransitionType transitionType;

    [SerializeField]
    private TravelType travelType;

    [SerializeField]
    [Range(32, 128)]
    private int distanceToReactivatePX = 32;

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

    private void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<LinearAnimator>();
        animator.Animate();
        Activate(activated);
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<LinearAnimator>();
        animator.Animate();
        Activate(activated);
    }

    public void AnimateAndTeleportToNextScene(GameObject target)
    {
        if (target.TryGetComponent(out PlayerController p))
        {
            p.DisableInput();
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

    public void Enter()
    {
        Activate(false);

        var player = FindAnyObjectByType<PlayerController>();

        if (player != null)
        {
            RunTeleportationAnimation(player, true);
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
            if (teleportIn)
            {
                target.gameObject.SetActive(false);
                target.transform.position = transform.position;
            }

            ta.Animate(() =>
            {
                // On Cover
                target.gameObject.SetActive(teleportIn);
                target.DisableInput();
            }, () =>
            {
                // On Done
                if (teleportIn)
                {
                    StartCoroutine(sr.FadeOut(fadeDuration, fadeSteps, () =>
                    {
                        // On Done
                        animator.Stop();

                        if (travelType == TravelType.Bidirectional)
                        {
                            StartCoroutine(WatchPlayerDistance(target.gameObject));
                        }

                        target.EnableInput();
                    }));
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
        while (Vector2.Distance(target.transform.position, transform.position) < distanceToReactivatePX)
        {
            yield return null;
        }

        void onDone()
        {
            // On Done
            animator.Animate();
            Activate(true);
        }

        StartCoroutine(sr.FadeIn(fadeDuration, fadeSteps, onDone));
    }

    private void Activate(bool activate)
    {
        activated = activate;

        if (TryGetComponent(out Collider2D collider2D))
        {
            collider2D.enabled = activate;
        }
    }
}
