using System;
using UnityEngine;

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

    public Action onTriggerEnter;
    public Action onTriggerExit;

    [SerializeField] private Strategy strategy;

    private GameObject triggerer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent(out ITriggerer _) || other.gameObject == triggerer) return;

        triggerer = other.gameObject;

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
        if (!other.gameObject.TryGetComponent(out ITriggerer _) ||
            other.gameObject != triggerer ||
            triggerer == null)
        {
            return;
        }

        triggerer = null;

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
