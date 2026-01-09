using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Teleport : MonoBehaviour, ILoggerProvider, ISceneTransitionPoint
{
    [SerializeField]
    private bool activated = false;

    [SerializeField]
    private Sprite inactiveSprite;

    [SerializeField]
    private GameObject teleportingAnimationTemplate;

    [SerializeField]
    private TransitionType transitionType;

    [SerializeField]
    private Logger logger;

    private LinearAnimator animator;

    public Logger Logger => logger;

    public Action<TransitionType> OnExit { get; set; }

    public TransitionType Type => transitionType;

    private void OnEnable()
    {
        animator = GetComponent<LinearAnimator>();
    }

    private void Awake()
    {
        if (activated)
        {
            animator.Animate();
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = inactiveSprite;
        }
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
                Exit(target);
            });
        }
        else
        {
            logger.E($"{target.name}: no {nameof(AutoMover)} attached!!");
        }
    }

    private void OnValidate()
    {
        UpdateActivatedState();
    }

    private void UpdateActivatedState()
    {
        if (activated)
        {
            if (TryGetComponent(out animator))
            {
                animator.Animate();
            }
        }
        else
        {
            if (TryGetComponent(out animator))
            {
                animator.Stop();
            }
        }

        if (TryGetComponent(out Collider2D collider2D))
        {
            collider2D.enabled = activated;
        }
    }

    public void Enter()
    {
        var player = FindAnyObjectByType<PlayerController>();

        if (player != null)
        {
            player.gameObject.SetActive(false);
            player.transform.position = transform.position;
            RunTeleportationAnimation(player.gameObject, () =>
            {
                // On Cover
                player.gameObject.SetActive(true);
            }, null);
        }
        else
        {
            logger.E($"{nameof(PlayerController)} not found!!");
        }
    }

    private void Exit(GameObject target)
    {
        RunTeleportationAnimation(target, () =>
                {
                    // On Cover
                    target.SetActive(false);
                }, () =>
                {
                    // On Done
                    OnExit?.Invoke(transitionType);
                });
    }

    private void RunTeleportationAnimation(GameObject target, Action onCover, Action onDone)
    {
        var go = Instantiate(teleportingAnimationTemplate, transform);

        if (go.TryGetComponent(out TeleportingAnimator ta))
        {
            ta.Animate(() =>
            {
                // On Cover
                onCover?.Invoke();
                animator.Stop();
            }, () =>
            {
                // On Done
                onDone?.Invoke();
            });
        }
        else
        {
            logger.E($"template ({teleportingAnimationTemplate.name}): no {nameof(TeleportingAnimator)} attached!");
        }
    }
}
