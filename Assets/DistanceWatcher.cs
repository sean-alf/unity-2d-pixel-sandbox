using UnityEngine;
using UnityEngine.Events;

public class DistanceWatcher : MonoBehaviour
{
    [SerializeField]
    private GameObject toBeWatched;

    [SerializeField]
    [Range(0, 512)]
    private int pixelDistance = 256;

    [SerializeField]
    private UnityEvent onActivate;

    [SerializeField]
    private UnityEvent onDeactivate;

    private Vector2 fixedPosition;
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

    private void OnEnable()
    {
        fixedPosition = transform.position;
    }

    private void Update()
    {
        if (!toBeWatched.activeSelf)
        {
            Activated = false;
            return;
        }

        Activated = Vector2.Distance(toBeWatched.transform.position, fixedPosition) < pixelDistance;
    }
}
