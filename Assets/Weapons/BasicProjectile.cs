using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BasicProjectile : MonoBehaviour, ILogTagProvider
{
    private static readonly ProjectileAnimationStateManager animationStateManager = new();

    [SerializeField]
    private ProjectileAnimationStateManager.ProjectileID id = ProjectileAnimationStateManager.ProjectileID.UNSET;

    [SerializeField]
    [Range(1, 100)]
    private int speed = 1;

    [Header("Debug")]
    [Space]

    [SerializeField]
    private LogLevelSelector logLevelSelector;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Vector2 direction;
    private Logging.Tag logTag;
    private bool move = true;
    private int halfHeight;

    public Logging.Tag LogTag => logTag;

    private void OnEnable()
    {
        logTag = this.CreateLogTag();
        Logging.SetLogLevel(logTag, logLevelSelector.logLevel);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        halfHeight = (int)sr.size.y / 2;
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
            Logging.LogError(logTag, $"finish state name null!");
        }
    }

    public void Use(Vector2 direction, Vector3 startPosition)
    {
        var results = new List<Collider2D>();

        if (Physics2D.OverlapCollider(GetComponent<Collider2D>(), results) > 0)
        {
            // Prevent using, just play dissipation animation
            var stateName = animationStateManager.GetAnimationData(id).finishStateName;
            animator.Play(stateName);
            return;
        }

        move = true;

        this.direction = direction;
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        transform.SetPositionAndRotation(startPosition.Add(halfHeight * direction.normalized), Quaternion.Euler(0, 0, angle));
    }

    public void Animator_Finish()
    {
        Destroy(gameObject);
    }
}
