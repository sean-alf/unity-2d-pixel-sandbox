using System;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Action onSwitchedTo;
    public Action onSwitchedBack;

    [SerializeField] private new TargetFollowingCamera camera;
    [SerializeField] private float maxSpeed;

    // For UnityEvents
    public void SwitchTo()
    {
        camera.SetNewTarget(transform, maxSpeed, () => onSwitchedTo?.Invoke());
    }

    public void SwitchTo(Transform target)
    {
        if (target.TryGetComponent(out Destroyable d))
        {
            d.onDestroyed += OnProjectileDestroyed;
        }
        camera.SetNewTarget(target.transform, maxSpeed, () => onSwitchedTo?.Invoke());
    }

    public void SwitchBack()
    {
        camera.SetOriginalTarget(maxSpeed, () => onSwitchedBack?.Invoke());
    }

    private void OnProjectileDestroyed()
    {
        camera.SetOriginalTarget(maxSpeed, () => onSwitchedBack?.Invoke());
    }
}
