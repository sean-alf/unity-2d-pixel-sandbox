using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Add(this Vector3 v, Vector2 v2)
    {
        return new(v.x + v2.x, v.y + v2.y, v.z);
    }

    public static Vector3 Add(this Vector3 v, float x, float y)
    {
        return new(v.x + x, v.y + y, v.z);
    }

    public static float DistanceX(this Vector3 v, Vector3 other) => Mathf.Abs(other.x - v.x);

    public static float DistanceY(this Vector3 v, Vector3 other) => Mathf.Abs(other.y - v.y);

    public static Vector3 Add(this Vector3 v, Vector3 other) => v + other;

    public static Vector3 Subtract(this Vector3 v, Vector3 other) => v - other;

    public static Vector3 Direction(this Vector3 v, Vector3 other) => other.Subtract(v).normalized;
}