using UnityEngine;
using UnityEngine.Events;

public class DirectionWatcher : MonoBehaviour
{
    public Transform target;
    [SerializeField] private bool shouldWatch;
    [SerializeField] private bool sendZeroVectorOnStopWatching = true;
    public UnityEvent<Vector2> onNewDirection;

    [Space]
    [Header("Debug")]

    [SerializeField] private Vector2 currentDirection;

    private void Update()
    {
        if (!shouldWatch || target == null) return;

        Vector2 newDirection = transform.position.Direction(target.transform.position);

        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            onNewDirection?.Invoke(newDirection);
        }
    }

    public void StartWatching() => shouldWatch = true;

    public void StopWatching()
    {
        shouldWatch = false;
        currentDirection = Vector2.zero;
        if (sendZeroVectorOnStopWatching) onNewDirection?.Invoke(currentDirection);
    }
}
