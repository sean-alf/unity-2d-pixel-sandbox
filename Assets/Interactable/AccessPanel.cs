using System.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(SpriteResolver))]
public class AccessPanel : MonoBehaviour, ILoggerProvider
{
    // Exposed for ST2U prefab replacer
    public string targetID; // The id (gameObject.name) of the other thing to access

    [Space]
    [Header("Debug")]

    [SerializeField]
    private Logger logger;

    private SpriteResolver sr;
    private bool activated = false;

    public Logger Logger => logger;

    private void Awake()
    {
        sr = GetComponent<SpriteResolver>();

        sr.SetCategoryAndLabel("Button", "Unpressed");
    }

    public void Interactable_Interact(GameObject _)
    {
        Activate();
    }

    private void Activate()
    {
        if (activated) return;
        activated = true;

        var targets = FindObjectsByType<RemoteInteractable>(FindObjectsSortMode.None);

        if (targets.Count() == 0)
        {
            logger.W($"no objects with {nameof(RemoteInteractable)} attached found!");
            return;
        }

        if (TryGetComponent(out LinearAnimator a))
        {
            a.Animate(() =>
            {
                // On Done
                var targetIDs = targetID.Split(",");

                foreach (var id in targetIDs)
                {
                    foreach (var target in targets)
                    {
                        target.Interact(id);
                    }
                }
            });
        }
    }
}
