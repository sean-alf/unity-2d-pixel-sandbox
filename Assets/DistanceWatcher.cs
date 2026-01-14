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
    private int pixelsPerUnit = 32;

    [SerializeField]
    private UnityEvent onActivate;

    [SerializeField]
    private UnityEvent onDeactivate;

    private Vector2 fixedPosition;
    private float scaledPixels;
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

    private void Awake()
    {
        fixedPosition = transform.position;
        scaledPixels = pixelDistance / (float)pixelsPerUnit;
    }

    private void Update()
    {
        if (!toBeWatched.activeSelf)
        {
            Activated = false;
            return;
        }

        Activated = Vector2.Distance(toBeWatched.transform.position, fixedPosition) < scaledPixels;
    }
}
