using System;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class TargetFollowerCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    private new Camera camera;

    private Transform currentTarget;
    private Vector3 velocity = Vector3.zero;
    private Action onCentered;
    private bool catchUp = false;
    private float maxSpeed = 5;

    void Awake()
    {
        camera = GetComponent<Camera>();
        currentTarget = target;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            currentTarget = target;
        }
        else
        {
            EditorApplication.delayCall += () =>
            {
                currentTarget = target;
            };
        }
    }
#endif

    void LateUpdate()
    {
        if (currentTarget == null || camera == null) return;

        if (catchUp)
        {
            var distance = Vector2.Distance(currentTarget.position, transform.position);
            Vector3 targetPosition = new(currentTarget.position.x, currentTarget.position.y, transform.position.z);

            if (distance >= 0.01)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref velocity,
                    smoothTime: 0.15f,
                    maxSpeed,
                    Time.unscaledDeltaTime
                );
            }
            else
            {
                transform.position = targetPosition;
                velocity = Vector3.zero;
                catchUp = false;
                onCentered?.Invoke();
            }
        }
        else
        {
            transform.position = new(currentTarget.position.x, currentTarget.position.y, transform.position.z);
        }
    }

    public void SetNewTarget(Transform newFollow, float maxSpeed, Action onCentered)
    {
        this.onCentered = onCentered;
        this.maxSpeed = maxSpeed;
        catchUp = true;
        currentTarget = newFollow;
    }

    public void SetOriginalTarget(float maxSpeed, Action onCentered)
    {
        this.onCentered = onCentered;
        this.maxSpeed = maxSpeed;
        catchUp = true;
        currentTarget = target;
    }
}
