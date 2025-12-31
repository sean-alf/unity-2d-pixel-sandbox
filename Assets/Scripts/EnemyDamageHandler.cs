using UnityEngine;

public class EnemyDamageHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject deathCloudTemplate;

    [SerializeField]
    private int health = 1;

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log($"EnemyDamageHandler: Collision {other.gameObject.name}");

        if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            Debug.Log($"EnemyDamageHandler: has CollisionData");

            if (data.receivers.Contains(CollisionData.Receiver.Enemy))
            {
                Debug.Log($"EnemyDamageHandler: receiver is Enemy");

                if (data.type == CollisionData.Type.Damage)
                {
                    Debug.Log($"EnemyDamageHandler: type is Damage");
                    Debug.Log($"EnemyDamageHandler: strength {data.strength}");

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
