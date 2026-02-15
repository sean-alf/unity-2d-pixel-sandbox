using UnityEngine;
using UnityEngine.Events;

public class DistanceWatcher : MonoBehaviour
{
    [SerializeField] private GameObject toBeWatched;
    [SerializeField][Range(0, 32)] private float distance = 8;
    [SerializeField] private bool isWatching = true;
    [SerializeField] private UnityEvent onActivate;
    [SerializeField] private UnityEvent onDeactivate;

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
        if (!toBeWatched.activeSelf || !isWatching)
        {
            Activated = false;
            return;
        }

        Activated = Vector2.Distance(toBeWatched.transform.position, transform.position) < distance;
    }
}
