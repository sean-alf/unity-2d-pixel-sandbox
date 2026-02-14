using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteFlasher))]
public class EnemyDamageHandler : MonoBehaviour, ILoggerProvider
{
    public UnityEvent onKnockbackStart;
    public UnityEvent onKnockbackEnd;
    public UnityEvent onDeathPreAnimate;
    public UnityEvent onDeath;

    [SerializeField] private GameObject deathCloudTemplate;
    [SerializeField] private int health = 1;
    [SerializeField] private List<CollisionData> onlyDamagableBy;

    [Header("Stationary Enemies")]
    [SerializeField] private float damageTakenDelay = 0.25f;

    [Header("Debug")]
    [Space]

    [SerializeField] private Logger logger;

    public Logger Logger => logger;

    private new Collider2D collider;
    private KnockbackReceiver knockbackReceiver;
    private SpriteFlasher spriteFlasher;

    void Awake()
    {
        collider = GetComponent<Collider2D>();
        // Some enemies (like fixed enemies, e.g. Globbels) do NOT have a knockback receiver
        TryGetComponent(out knockbackReceiver);
        spriteFlasher = GetComponent<SpriteFlasher>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        logger.D($"{name} ({gameObject.name}): OnCollisionEnter2D {other.gameObject.name}");

        if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            logger.D($"has CollisionData");

            var canTakeDamage = onlyDamagableBy == null ||
               onlyDamagableBy.Count == 0 ||
               onlyDamagableBy.Find(d => d.ID == data.ID);

            if (canTakeDamage && data.Type == CollisionData.CollisionType.Damage)
            {
                TakeDamage(data, other);
            }
        }
    }

    private void TakeDamage(CollisionData data, Collision2D other)
    {
        logger.D($"type is Damage");
        logger.D($"strength {data.Strength}");

        health -= data.Strength;

        spriteFlasher.StartFlash();

        if (knockbackReceiver != null)
        {
            // Handle moving enemies
            onKnockbackStart?.Invoke();
            knockbackReceiver.KnockBack(data.Strength, other, onDone: () =>
            {
                spriteFlasher.StopFlash();
                CheckHealth();
                onKnockbackEnd?.Invoke();
            });
        }
        else
        {
            // Handle stationary enemies
            this.StartTimer(damageTakenDelay, onExpired: () =>
            {
                spriteFlasher.StopFlash();
                CheckHealth();
            });
        }
    }

    private void CheckHealth()
    {
        if (health <= 0)
        {
            onDeathPreAnimate?.Invoke();
            collider.enabled = false;
            var deathCloud = Instantiate(deathCloudTemplate, transform).GetComponent<DeathCloud>();
            deathCloud.onAnimationEnd += Die;
            deathCloud.Begin();
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}
