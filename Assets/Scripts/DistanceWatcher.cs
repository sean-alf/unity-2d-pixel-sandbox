using UnityEngine;
using UnityEngine.Events;

public class DistanceWatcher : MonoBehaviour
{
    public Transform target;
    [SerializeField][Range(0, 32)] private float distanceToActivate = 8;
    [SerializeField][Tooltip("The distance amount greater than distanceToActivate to trigger deactivation")][Range(0, 32)] private float deactivationDistanceHysteresis = 0;
    [SerializeField] private bool isWatching = true;
    [SerializeField] private UnityEvent onActivate;
    [SerializeField] private UnityEvent onDeactivate;

    [Space]
    [Header("Debug")]

    [SerializeField] private float totalDeactivationDistance;

    private void Awake()
    {
        totalDeactivationDistance = distanceToActivate + deactivationDistanceHysteresis;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        totalDeactivationDistance = distanceToActivate + deactivationDistanceHysteresis;
    }
#endif

    private bool _isActivated = false;
    private bool Activated
    {
        set
        {
            if (_isActivated == value) return;

            if (value)
            {
                onActivate?.Invoke();
            }
            else
            {
                onDeactivate?.Invoke();
            }
            _isActivated = value;
        }
    }

    public void StopWatching() => isWatching = false;

    public void StartWatching() => isWatching = true;

    private void Update()
    {
        if (!target.gameObject.activeSelf || !isWatching)
        {
            Activated = false;
            return;
        }

        float distance = Vector2.Distance(target.position, transform.position);

        if (distance <= distanceToActivate)
        {
            Activated = true;
        }
        else if (distance >= totalDeactivationDistance)
        {
            Activated = false;
        }
    }
}
