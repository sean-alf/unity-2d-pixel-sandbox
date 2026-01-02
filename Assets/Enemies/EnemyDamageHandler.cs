using UnityEngine;

public class EnemyDamageHandler : MonoBehaviour, ILogTagProvider
{
    [SerializeField]
    private GameObject deathCloudTemplate;

    [SerializeField]
    private int health = 1;

    [Header("Debug")]
    [Space]

    [SerializeField]
    private LogLevelSelector logLevelSelector;

    private Logging.Tag logTag;

    public Logging.Tag LogTag => logTag;

    private void OnEnable()
    {
        logTag = this.CreateLogTag();
        Logging.SetLogLevel(logTag, logLevelSelector.logLevel);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Logging.LogDebug(logTag, $"Collision {other.gameObject.name}");

        if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            Logging.LogDebug(logTag, $"has CollisionData");

            if (data.receivers.Contains(CollisionData.Receiver.Enemy))
            {
                Logging.LogInfo(logTag, $"receiver is Enemy");

                if (data.type == CollisionData.Type.Damage)
                {
                    Logging.LogInfo(logTag, $"type is Damage");
                    Logging.LogInfo(logTag, $"strength {data.strength}");

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
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
