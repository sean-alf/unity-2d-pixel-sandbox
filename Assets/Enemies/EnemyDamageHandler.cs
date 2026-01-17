using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamageHandler : MonoBehaviour, ILoggerProvider
{
    public UnityEvent onDeathPreAnimate;

    public UnityEvent onDeath;

    [SerializeField]
    private GameObject deathCloudTemplate;

    [SerializeField]
    private int health = 1;

    [SerializeField]
    private List<CollisionData> onlyDamagableBy;

    [Header("Debug")]
    [Space]

    [SerializeField]
    private Logger logger;

    public Logger Logger => logger;

    private new Collider2D collider;

    void Awake()
    {
        collider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        logger.D($"Collision {other.gameObject.name}");

        if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            logger.D($"has CollisionData");

            if (onlyDamagableBy == null || onlyDamagableBy.Count == 0 || onlyDamagableBy.Find(d => d.ID == data.ID))
            {
                if (data.Type == CollisionData.CollisionType.Damage)
                {
                    logger.D($"type is Damage");
                    logger.D($"strength {data.Strength}");

                    health -= data.Strength;

                    if (health <= 0)
                    {
                        onDeathPreAnimate?.Invoke();
                        collider.enabled = false;
                        var deathCloud = Instantiate(deathCloudTemplate, transform).GetComponent<DeathCloud>();
                        deathCloud.onAnimationEnd += Die;
                        deathCloud.Begin();
                    }
                }
            }
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}
