using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(Camera))]
public class PlayerFollowingCamera : MonoBehaviour
{
    [SerializeField]
    private Transform follow;

    private new Camera camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        camera.transform.position = new(follow.position.x, follow.position.y, camera.transform.position.z);
    }
}
