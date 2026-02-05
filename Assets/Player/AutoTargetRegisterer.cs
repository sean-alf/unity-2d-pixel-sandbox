using UnityEngine;

[ExecuteAlways]
public class AutoTargetRegisterer : MonoBehaviour
{
    [Header("This component automatically registers the player game object as the main target for the TargetFollowerCamera.\nThis includes in the editor as well.")]
    [Space]
    [SerializeField] private TargetFollowerCameraRequestEvent targetFollowerCameraRequestEvent;

    private void OnEnable()
    {
        targetFollowerCameraRequestEvent.Raise(new(
            command: TargetFollowerCamera.Command.SetMainTarget,
            target: transform,
            maxSpeed: 0 // ignore
        ));
    }
}
