using UnityEngine;

public class EnemyDamageReceiver : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.IsOnLayer(LayerNames.Projectile))
        {
            Destroy(gameObject);
        }
    }
}
