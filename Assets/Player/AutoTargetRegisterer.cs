using UnityEngine;

[ExecuteAlways]
public class AutoTargetRegisterer : MonoBehaviour
{
    [Header("This component automatically registers the player game object as the main target for the TargetFollowerCamera.")]
    [Space]
    [SerializeField] private TargetFollowerCameraRequestEvent targetFollowerCameraRequestEvent;

#if UNITY_EDITOR
    private TargetFollowerCamera targetFollowerCamera;

    private void Awake() => FindCameraAndSetTarget();

    private void OnEnable() => FindCameraAndSetTarget();

    private void OnValidate() => FindCameraAndSetTarget();

    private void FindCameraAndSetTarget()
    {
        if (Application.isPlaying) return;

        var mainCamera = GameObject.Find("Main Camera");

        if (mainCamera)
        {
            targetFollowerCamera = mainCamera.GetComponent<TargetFollowerCamera>();
            targetFollowerCamera.UnityEvent_SetRequest(new(
                command: TargetFollowerCamera.Command.SetMainTarget,
                target: transform,
                maxSpeed: 0 // ignore
            ));
        }
    }

    private void Update()
    {
        if (Application.isPlaying ||
            targetFollowerCamera == null ||
            (targetFollowerCamera.CurrentTarget != null && targetFollowerCamera.CurrentTarget.gameObject == gameObject))
        {
            return;
        }
        Debug.Log($"{name} ({GetType().Name}): setting camera target");
        targetFollowerCamera.UnityEvent_SetRequest(new(
            command: TargetFollowerCamera.Command.SetMainTarget,
            target: transform,
            maxSpeed: 0 // ignore
        ));
    }
#endif


    private void Start()
    {
        // This must be done here so that the player will get set as the main target for the camera
        // during game play
        targetFollowerCameraRequestEvent.Raise(new(
                command: TargetFollowerCamera.Command.SetMainTarget,
                target: transform,
                maxSpeed: 0 // ignore
            ));
    }
}
