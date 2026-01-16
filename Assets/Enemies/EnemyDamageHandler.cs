using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamageHandler : MonoBehaviour, ILoggerProvider
{
    public UnityEvent onDeathPreAnimate;

    [SerializeField]
    private GameObject deathCloudTemplate;

    [SerializeField]
    private int health = 1;

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

            if (data.type == CollisionData.Type.Damage)
            {
                logger.I($"type is Damage");
                logger.I($"strength {data.strength}");

                health -= data.strength;

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

    private void Die()
    {
        Destroy(gameObject);
    }
}
