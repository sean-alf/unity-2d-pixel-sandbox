using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Teleport : MonoBehaviour, ILogTagProvider
{
    public Logging.Tag LogTag => logTag;

    [SerializeField]
    private bool activated = false;

    [SerializeField]
    private Sprite inactiveSprite;

    [SerializeField]
    private GameObject teleportingAnimationTemplate;

    [SerializeField]
    private LogLevelSelector logLevelSelector;

    private Logging.Tag logTag;
    private LinearAnimator animator;

    private void OnEnable()
    {
        logTag = this.CreateLogTag();
        Logging.SetLogLevel(logTag, logLevelSelector.logLevel);

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

    public void AnimateAndTeleportToNextScene(GameObject go)
    {
        if (go.TryGetComponent(out PlayerController p))
        {
            p.DisableInput();
        }
        else
        {
            Logging.LogError(logTag, $"{go.name}: no {nameof(PlayerController)} attached!!");
        }

        if (go.TryGetComponent(out AutoMover a))
        {
            a.MoveTo(transform.position, () =>
            {
                // OnDone
                var go = Instantiate(teleportingAnimationTemplate, transform);

                if (go.TryGetComponent(out TeleportingAnimator ta))
                {
                    ta.Animate(() =>
                    {
                        // On Cover
                        // Hide the player
                        p.gameObject.SetActive(false);
                        animator.Stop();
                    }, () =>
                    {
                        Logging.LogDebug(logTag, "TODO: Move to next scene");
                    });
                }
                else
                {
                    Logging.LogError(logTag, $"template ({teleportingAnimationTemplate.name}): no {nameof(TeleportingAnimator)} attached!");
                }
            });
        }
        else
        {
            Logging.LogError(logTag, $"{go.name}: no {nameof(AutoMover)} attached!!");
        }
    }

    private void OnValidate()
    {
        Logging.SetLogLevel(logTag, logLevelSelector.logLevel);
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
}
