using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BasicProjectile : MonoBehaviour, ILoggerProvider
{
    private static readonly ProjectileAnimationStateManager animationStateManager = new();

    [SerializeField]
    private ProjectileAnimationStateManager.ProjectileID id = ProjectileAnimationStateManager.ProjectileID.UNSET;

    [SerializeField]
    [Range(1, 100)]
    private int speed = 1;

    [SerializeField]
    [Range(-16, 16)]
    private int spawnPositionOffsetPX;

    [Header("Debug")]
    [Space]

    [SerializeField]
    private Logger logger;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Vector2 direction;
    private bool move = true;
    private int halfHeight;

    public Logger Logger => logger;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        halfHeight = (int)sr.bounds.size.y / 2;
    }

    private void FixedUpdate()
    {
        if (move)
        {
            rb.MovePosition((speed * direction) + rb.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        move = false;

        var stateName = animationStateManager.GetAnimationData(id).finishStateName;

        if (stateName != null)
        {
            animator.Play(stateName);
        }
        else
        {
            logger.E($"finish state name null!");
        }
    }

    public void Use(Vector2 direction, Vector3 startPosition)
    {
        float angleDegrees = Vector2.SignedAngle(Vector2.up, direction);
        Use(startPosition, direction, Quaternion.Euler(0, 0, angleDegrees));
    }

    public void Use(float angleDegrees, Vector3 startPosition)
    {
        float rads = angleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new(Mathf.Cos(rads), Mathf.Sin(rads));
        Use(startPosition, direction, Quaternion.Euler(0, 0, angleDegrees));
    }

    public void UseRads(float angleRads, Vector3 startPosition)
    {
        Vector2 direction = new(Mathf.Cos(angleRads), Mathf.Sin(angleRads));
        Use(startPosition, direction, Quaternion.Euler(0, 0, angleRads * Mathf.Rad2Deg));
    }

    private void Use(Vector3 startPosition, Vector2 direction, Quaternion rotation)
    {
        transform.SetPositionAndRotation(startPosition.Add((halfHeight + spawnPositionOffsetPX) * direction.normalized), rotation);
        this.direction = direction;
        move = true;
    }

    public void Animator_Finish()
    {
        Destroy(gameObject);
    }
}
