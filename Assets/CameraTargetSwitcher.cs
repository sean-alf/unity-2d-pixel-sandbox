using UnityEngine;
using UnityEngine.Events;

public class CameraTargetSwitcher : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private new PlayerFollowingCamera camera;
    [SerializeField] private float maxSpeed;
    [SerializeField] private UnityEvent onSwitchedTo;
    [SerializeField] private UnityEvent onSwitchedBack;

    public void SwitchTo(BasicProjectile p)
    {
        p.onDestroyed += OnProjectileDestroyed;
        camera.SetFollow(p.transform, maxSpeed, () => onSwitchedTo?.Invoke());
    }

    private void OnProjectileDestroyed()
    {
        camera.SetFollow(playerTransform, maxSpeed, () => onSwitchedBack?.Invoke());
    }
}
