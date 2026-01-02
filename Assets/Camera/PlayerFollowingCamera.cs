using UnityEngine;

[ExecuteAlways]
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
        if (follow == null || camera == null) return;

        camera.transform.position = new(follow.position.x, follow.position.y, camera.transform.position.z);
    }
}
