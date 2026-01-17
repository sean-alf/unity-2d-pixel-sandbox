using UnityEngine;

public class ReflectingBlock : MonoBehaviour
{
    public void HandleReflect(Collision2D collision) => collision.otherRigidbody.WhenNotNull(rb => rb.linearVelocity = Vector2.Reflect(
        -collision.relativeVelocity,
        collision.contacts[0].normal
    ));
}
