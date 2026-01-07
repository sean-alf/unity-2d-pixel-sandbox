using UnityEngine;
using UnityEngine.Rendering;

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
        Logging.LogDebug(logTag, "Teleport begin!!");

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
                // TODO: Now animate the player out and switch to next scene
                Logging.LogDebug(logTag, "Teleport the player!!!");
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
