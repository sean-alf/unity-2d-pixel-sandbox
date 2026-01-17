using UnityEngine;
using UnityEngine.Events;

public interface ITriggerer { }

public class BasicTrigger : MonoBehaviour
{
    public enum Strategy
    {
        Nothing,
        DisableAfterEnter,
        DestroyAfterEnter,
        DisableAfterExit,
        DestroyAfterExit,
    }

    [SerializeField]
    private UnityEvent onTriggerEnter;

    [SerializeField]
    private UnityEvent onTriggerExit;

    [SerializeField]
    private Strategy strategy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent(out ITriggerer _)) return;

        onTriggerEnter?.Invoke();

        switch (strategy)
        {
            case Strategy.DisableAfterEnter:
                if (TryGetComponent(out Collider2D c)) c.enabled = false;
                enabled = false;
                break;
            case Strategy.DestroyAfterEnter:
                Destroy(gameObject);
                break;
            default:
                // Do nothing
                break;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent(out ITriggerer _)) return;

        onTriggerExit?.Invoke();

        switch (strategy)
        {
            case Strategy.DisableAfterExit:
                if (TryGetComponent(out Collider2D c)) c.enabled = false;
                enabled = false;
                break;
            case Strategy.DestroyAfterExit:
                Destroy(gameObject);
                break;
            default:
                // Do nothing
                break;
        }
    }
}
