using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Teleport : MonoBehaviour, ILogTagProvider
{
    public Logging.Tag LogTag => logTag;


    [SerializeField]
    private LogLevelSelector logLevelSelector;

    private Logging.Tag logTag;

    private void OnEnable()
    {
        logTag = this.CreateLogTag();
        Logging.SetLogLevel(logTag, logLevelSelector.logLevel);
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
    }
}
