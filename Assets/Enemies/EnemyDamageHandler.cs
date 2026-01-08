using UnityEngine;

public class EnemyDamageHandler : MonoBehaviour, ILoggerProvider
{
    [SerializeField]
    private GameObject deathCloudTemplate;

    [SerializeField]
    private int health = 1;

    [Header("Debug")]
    [Space]

    [SerializeField]
    private Logger logger;

    public Logger Logger => logger;

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
