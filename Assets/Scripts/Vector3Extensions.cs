using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Add(this Vector3 v, Vector3 other) => v + other;
    public static Vector3 Add(this Vector3 v, Vector2 v2) => new(v.x + v2.x, v.y + v2.y, v.z);
    public static Vector3 Add(this Vector3 v, float x, float y) => new(v.x + x, v.y + y, v.z);

    public static Vector3 Subtract(this Vector3 v, Vector3 other) => v - other;
    public static Vector3 Subtract(this Vector3 v, float x, float y) => new(v.x - x, v.y - y, v.z);
    public static Vector3 SubtractY(this Vector3 v, float y) => new(v.x, v.y - y, v.z);

    public static float DistanceX(this Vector3 v, Vector3 other) => Mathf.Abs(other.x - v.x);

    public static float DistanceY(this Vector3 v, Vector3 other) => Mathf.Abs(other.y - v.y);

    public static Vector3 Direction(this Vector3 v, Vector3 other) => other.Subtract(v).normalized;

    public static Vector3 Round(this Vector3 v) => new(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));

    public static Vector3 SnapXYToHalfInteger(this Vector3 v)
    {
        var v2 = new Vector2(Mathf.Round(v.x * 2f), Mathf.Round(v.y * 2f)) / 2f;
        return new(v2.x, v2.y, v.z);
    }

    public static Vector2 ToVector2(this Vector3 v) => (Vector2)v;
}