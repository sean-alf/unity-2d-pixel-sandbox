using System.Collections.Generic;
using UnityEngine;

public class ReflectingBlock : MonoBehaviour
{
    [SerializeField]
    private CardinalDirection direction;

    private readonly Dictionary<BasicProjectile, float> incomingSpeeds = new();

    private void Awake() => UpdateRotation();

    private void OnValidate() => UpdateRotation();

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If it's a projectile, then cached a reference to it
        if (other.gameObject.TryGetComponent(out BasicProjectile p) && !incomingSpeeds.ContainsKey(p))
        {
            var v = other.attachedRigidbody.linearVelocity;
            incomingSpeeds.Add(p, Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y)));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out BasicProjectile p) && incomingSpeeds.ContainsKey(p)) incomingSpeeds.Remove(p);
    }

    private void FixedUpdate()
    {
        foreach (var item in incomingSpeeds)
        {
            var p = item.Key;

            if (p.TryGetComponent(out Rigidbody2D rb))
            {
                var v = rb.linearVelocity;
                bool movingX = Mathf.Abs(v.x) > Mathf.Abs(v.y);
                bool movingY = Mathf.Abs(v.y) > Mathf.Abs(v.x);
                bool movingBoth = Mathf.Abs(v.y) == Mathf.Abs(v.x);

                float xDistance = Mathf.Abs(item.Key.transform.position.x - transform.position.x);
                float yDistance = Mathf.Abs(item.Key.transform.position.y - transform.position.y);

                if (movingX && xDistance <= 0.1 || movingY && yDistance <= 0.1)
                {
                    rb.linearVelocity = item.Value * direction.ToVector2();
                }
                else if (movingBoth && xDistance <= 1f / 2f && yDistance <= 1f / 2f)
                {
                    rb.linearVelocity = item.Value * direction.ToVector2();
                }

                p.UpdateRotation();
            }
        }
    }

    private void UpdateRotation() => transform.rotation = Quaternion.LookRotation(Vector3.forward, direction.ToVector2());
}
