using UnityEngine;

[ExecuteAlways]
public class AutoTargetRegisterer : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("This component automatically registers the player game object as the main target for the TargetFollowerCamera.\nThis includes in the editor as well.")]
    [Space]
    [SerializeField] private TargetFollowerCameraRequestEvent targetFollowerCameraRequestEvent;

    private void OnEnable()
    {
        var go = GameObject.Find("Main Camera");
        if (go)
        {
            var targetFollowerCamera = go.GetComponent<TargetFollowerCamera>();
            targetFollowerCamera.UnityEvent_SetRequest(new(
                command: TargetFollowerCamera.Command.SetMainTarget,
                target: transform,
                maxSpeed: 0 // ignore
            ));
        }
    }
#endif

    private void Start()
    {
        targetFollowerCameraRequestEvent.Raise(new(
                command: TargetFollowerCamera.Command.SetMainTarget,
                target: transform,
                maxSpeed: 0 // ignore
            ));
    }
}
