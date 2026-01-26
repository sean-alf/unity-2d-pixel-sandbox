using System;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class PlayerFollowingCamera : MonoBehaviour
{
    [SerializeField] private Transform follow;

    private new Camera camera;

    private Vector3 velocity = Vector3.zero;
    private Action onCentered;
    private bool catchUp = false;
    private float maxSpeed = 5;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (follow == null || camera == null) return;

        if (catchUp)
        {
            var distance = Vector2.Distance(follow.position, transform.position);
            Vector3 targetPosition = new(follow.position.x, follow.position.y, transform.position.z);

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
            transform.position = new(follow.position.x, follow.position.y, transform.position.z);
        }
    }

    public void SetFollow(Transform newFollow, float maxSpeed, Action onCentered)
    {
        this.onCentered = onCentered;
        this.maxSpeed = maxSpeed;
        catchUp = true;
        follow = newFollow;
    }
}
