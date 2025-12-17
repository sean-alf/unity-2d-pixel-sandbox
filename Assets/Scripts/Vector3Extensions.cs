using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Add(this Vector3 v, Vector2 v2)
    {
        return new(v.x + v2.x, v.y + v2.y, v.z);
    }
}