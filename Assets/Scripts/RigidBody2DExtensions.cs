using UnityEngine;

public static class RigidBody2DExtensions
{
    /// <summary>
    /// Moves the Rigidbody2D towards the target position.
    /// Returns the value resulting from Vector2.MoveTowards().
    /// </summary>
    /// <param name="rb"></param>
    /// <param name="target"></param>
    /// <param name="maxDistanceDelta"></param>
    /// <returns></returns>
    public static Vector2 MoveTowards(this Rigidbody2D rb, Vector2 target, float maxDistanceDelta)
    {
        var v = Vector2.MoveTowards(rb.position, target, maxDistanceDelta);
        rb.MovePosition(v);
        return v;
    }

    public static float DistanceX(this Rigidbody2D rb, Vector2 target) => Mathf.Abs(target.x - rb.position.x);

    public static float DistanceY(this Rigidbody2D rb, Vector2 target) => Mathf.Abs(target.y - rb.position.y);
}
