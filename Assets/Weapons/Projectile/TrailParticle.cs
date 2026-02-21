using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TrailParticle : MonoBehaviour
{
    [SerializeField] private float speedScale = 0.3f;

    [Space]
    [Header("Debug")]

    [SerializeField] private float speed = 2f;
    [SerializeField] private Vector2 direction;

    public void Init(Vector2 dir, float inheritedSpeed)
    {
        direction = dir.normalized;
        speed = inheritedSpeed * speedScale;  // slightly slower than parent
    }

    void Update() => transform.position += (Vector3)(speed * Time.deltaTime * direction);
}