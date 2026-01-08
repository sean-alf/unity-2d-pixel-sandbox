using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Teleport : MonoBehaviour, ILoggerProvider
{
    [SerializeField]
    private bool activated = false;

    [SerializeField]
    private Sprite inactiveSprite;

    [SerializeField]
    private GameObject teleportingAnimationTemplate;

    [SerializeField]
    private string nextSceneName;

    [SerializeField]
    private Logger logger;

    private LinearAnimator animator;

    public Logger Logger => logger;

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

    public void AnimateAndTeleportToNextScene(GameObject go)
    {
        if (go.TryGetComponent(out PlayerController p))
        {
            p.DisableInput();
        }
        else
        {
            logger.E($"{go.name}: no {nameof(PlayerController)} attached!!");
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
                    }, async () =>
                    {
                        logger.D("TODO: Move to next scene, use persistent scene manager for this ultimately");
                        if (nextSceneName == null || nextSceneName.Length == 0)
                        {
                            logger.E("missing next scene name!");
                            return;
                        }

                        await SceneManager.LoadSceneAsync(nextSceneName);
                    });
                }
                else
                {
                    logger.E($"template ({teleportingAnimationTemplate.name}): no {nameof(TeleportingAnimator)} attached!");
                }
            });
        }
        else
        {
            logger.E($"{go.name}: no {nameof(AutoMover)} attached!!");
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
}
