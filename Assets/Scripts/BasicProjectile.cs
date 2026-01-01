using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class BasicProjectile : MonoBehaviour
{
    private static readonly int TO_EDGE_OF_EYE_PX = 4;

    [SerializeField]
    private ProjectileAnimationStateManager.ProjectileID id = ProjectileAnimationStateManager.ProjectileID.UNSET;

    [SerializeField]
    [Range(1, 100)]
    private int speed = 1;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Vector2 direction;
    private readonly ProjectileAnimationStateManager animationStateManager = new();
    private bool inUse = false;
    private bool move = true;
    private int halfHeight;

    public bool InUse => inUse;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        halfHeight = (int)sr.size.y / 2;
    }

    private void FixedUpdate()
    {
        if (inUse && move)
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
            Debug.LogError($"BasicProjectile ({gameObject.name}): finish state name null!");
        }
    }

    public void Use(Vector2 direction, Transform transform)
    {
        inUse = true;
        move = true;

        this.direction = direction;

        Vector3 position = transform.position;
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        this.transform.SetPositionAndRotation(position.Add((TO_EDGE_OF_EYE_PX + halfHeight) * direction.normalized), Quaternion.Euler(0, 0, angle));
        gameObject.SetActive(true);
    }

    public void Animator_Finish()
    {
        inUse = false;
        gameObject.SetActive(false);
    }
}
