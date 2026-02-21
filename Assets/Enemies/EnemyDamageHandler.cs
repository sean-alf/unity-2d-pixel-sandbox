using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteFlasher))]
public class EnemyDamageHandler : MonoBehaviour, ILoggerProvider
{
    [SerializeField] private GameObject deathCloudTemplate;
    [SerializeField] private int health = 1;
    [SerializeField] private List<CollisionData> onlyDamagableBy;
    [Tooltip("Requires calling DieIfDead() to cause death if health is 0")] public bool deferDeath = false;

    [Header("Stationary Enemies")]
    [SerializeField] private float damageCooldownPeriod = 0.25f;
    public UnityEvent onKnockbackStart;
    public UnityEvent onKnockbackEnd;
    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onDeathPreAnimate;
    public UnityEvent onDeath;

    [Header("Debug")]
    [Space]

    [SerializeField] private bool invincible = false;
    [SerializeField] private int currentHealth;
    [SerializeField] private Logger logger;

    public bool IsInvincible => invincible;
    public bool IsDead => currentHealth <= 0;
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

        currentHealth = health;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        logger.D($"{name} ({gameObject.name}): OnCollisionEnter2D {other.gameObject.name}, layer {LayerMask.LayerToName(other.gameObject.layer)}");

        // Generally, enemies shouldn't hurt other enemies
        if (other.gameObject.layer == LayerNames.EnemyIndex) return;

        if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            logger.D($"has CollisionData");

            var canTakeDamage = onlyDamagableBy == null ||
               onlyDamagableBy.Count == 0 ||
               onlyDamagableBy.Find(d => d.ID == data.ID);

            if (!invincible && canTakeDamage && data.Type == CollisionData.CollisionType.Damage)
            {
                TakeDamage(data, other);
            }
        }
    }

    /// <summary>
    /// Call this if deferDeath is true.
    /// </summary>
    /// <param name="onDone">If IsDead is true, gets called after the death animation completes, otherwise gets called immediately after checking health</param>
    public void DieIfDead(Action onDone) => CheckHealth(onDone);

    private void TakeDamage(CollisionData data, Collision2D other)
    {
        if (deferDeath && IsDead) return;

        logger.D($"type is Damage");
        logger.D($"strength {data.Strength}");

        currentHealth = Mathf.Max(currentHealth - data.Strength, 0);
        onHealthChanged?.Invoke(currentHealth, /*full health*/ health);
        invincible = true;
        spriteFlasher.StartFlash();

        if (knockbackReceiver != null)
        {
            // Handle moving enemies
            onKnockbackStart?.Invoke();
            knockbackReceiver.KnockBack(data.Strength, other, onDone: () =>
            {
                spriteFlasher.StopFlash();
                invincible = false;
                if (!deferDeath) CheckHealth();
                onKnockbackEnd?.Invoke();
            });
        }
        else
        {
            // Handle stationary enemies
            float duration = deferDeath && IsDead ? float.MaxValue : damageCooldownPeriod;

            this.StartTimer(duration, onExpired: () =>
            {
                spriteFlasher.StopFlash();
                invincible = false;
                if (!deferDeath) CheckHealth();
            });
        }
    }

    private void CheckHealth(Action onDone = null)
    {
        if (IsDead)
        {
            onDeathPreAnimate?.Invoke();
            collider.enabled = false;
            var deathCloud = Instantiate(deathCloudTemplate, transform).GetComponent<DeathCloud>();
            deathCloud.onAnimationEnd += Die;
            if (onDone != null) deathCloud.onAnimationEnd += onDone;
            deathCloud.Begin();
        }
        else
        {
            onDone?.Invoke();
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}
