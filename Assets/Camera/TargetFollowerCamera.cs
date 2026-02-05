using UnityEditor;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class TargetFollowerCamera : MonoBehaviour
{
    public enum Command
    {
        SetMainTarget,
        SetTemporaryTarget,
        SwitchBackToMainTarget,
    }

    public readonly struct TargetRequest
    {
        public readonly Command command;
        public readonly Transform target;
        public readonly float maxSpeed;

        public TargetRequest(Command command, Transform target, float maxSpeed)
        {
            this.command = command;
            this.target = target;
            this.maxSpeed = maxSpeed;
        }
    }

    [SerializeField] private TransformChangeEvent finishedCenteringTargetEvent;

    [Space]
    [Header("Debug")]

    [SerializeField] private Transform mainTarget;
    [SerializeField] private Transform currentTarget;
    [SerializeField] private float maxSpeed = 5;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private bool catchUp = false;

    void LateUpdate()
    {
        if (currentTarget == null) return;

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

                // This event may not be used in some cases
                if (finishedCenteringTargetEvent) finishedCenteringTargetEvent.Raise(currentTarget);
            }
        }
        else
        {
            transform.position = new(currentTarget.position.x, currentTarget.position.y, transform.position.z);
        }
    }

    public void UnityEvent_SetRequest(TargetRequest req)
    {
        if (req.maxSpeed > 0) maxSpeed = req.maxSpeed;

        switch (req.command)
        {
            case Command.SetMainTarget:
                if (req.target == null)
                {
                    Debug.LogError($"{name} ({GetType().Name}): req.target must not be null when command is {req.command}!");
                    return;
                }

                catchUp = false;
                mainTarget = req.target;
                currentTarget = req.target;
                break;
            case Command.SetTemporaryTarget:
                if (req.target == null)
                {
                    Debug.LogError($"{name} ({GetType().Name}): req.target must not be null when command is {req.command}!");
                    return;
                }

                catchUp = true;
                currentTarget = req.target;
                break;
            case Command.SwitchBackToMainTarget:
                if (mainTarget == null)
                {
                    Debug.LogError($"{name} ({GetType().Name}): main target must not be null when command is {req.command}!");
                    return;
                }

                catchUp = true;
                currentTarget = mainTarget;
                break;
        }
    }
}
