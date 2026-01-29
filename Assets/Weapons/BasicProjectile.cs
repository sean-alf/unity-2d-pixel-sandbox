using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BasicProjectile : MonoBehaviour, ILoggerProvider
{
    [SerializeField][Range(1, 40)] private int speed = 1;
    [SerializeField] private float destructionDelay = 0f;
    [SerializeField] private bool allowRotation = true;
    [SerializeField] private bool offsetForHalfHeight = true;
    [SerializeField] private string defaultAnimationKey = "Default";
    [SerializeField] private string impactAnimationKey = "Impact";

    [Header("Debug")]
    [Space]

    [SerializeField] private Logger logger;

    private Rigidbody2D rb;
    private LinearAnimator linearAnimator;
    private SpriteRenderer sr;
    private WaitForSeconds wait;
    private float halfHeight;

    public Logger Logger => logger;

    // ──────────────────────────────────────────────────────────────
    // GameObject Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        TryGetComponent(out linearAnimator);
        sr = GetComponent<SpriteRenderer>();

        halfHeight = sr.bounds.size.y / 2.0f;
        wait = new(destructionDelay);
    }

    private void Start()
    {
        if (ShouldAnimate(defaultAnimationKey)) linearAnimator.Animate(defaultAnimationKey);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        wait = new(destructionDelay);
    }
#endif

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out RedirectionPanel _)) return;
        StartCoroutine(DelayDestroy());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out RedirectionPanel _)) return;
        StartCoroutine(DelayDestroy());
    }

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    public void ClearVelocity() => rb.linearVelocity = Vector2.zero;

    public void UpdateRotation()
    {
        if (!allowRotation) return;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.linearVelocity);
    }

    public void Use(Vector2 direction, Vector3 startPosition)
    {
        float angleDegrees = Vector2.SignedAngle(Vector2.up, direction);
        Use(startPosition, direction, Quaternion.Euler(0, 0, angleDegrees));
    }

    private void Use(Vector3 startPosition, Vector2 direction, Quaternion rotation)
    {
        if (!allowRotation) rotation = Quaternion.identity;

        if (offsetForHalfHeight) startPosition = startPosition.Add(halfHeight * direction.normalized);

        transform.SetPositionAndRotation(startPosition, rotation);
        gameObject.SetActive(true);
        rb.linearVelocity = speed * direction;
    }

    // ──────────────────────────────────────────────────────────────
    // Coroutines
    // ──────────────────────────────────────────────────────────────

    private IEnumerator DelayDestroy()
    {
        yield return wait;

        rb.linearVelocity = Vector2.zero;

        if (linearAnimator == null)
        {
            Destroy(gameObject);
            yield return null;
        }

        if (ShouldAnimate(impactAnimationKey)) linearAnimator.Animate(impactAnimationKey,
            onFinished: () => Destroy(gameObject)
        );
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Not all projectiles will necessarily have animations.
    /// Only animate if there is an animator attached and the key is valid.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ShouldAnimate(string key) => linearAnimator && linearAnimator.IsKeyValid(key);
}
