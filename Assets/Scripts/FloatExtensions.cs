using UnityEngine;

public static class FloatExtensions
{
    public static bool IsSameDirection(this float f, float other)
    {
        return (f > 0 && other > 0) || (f < 0 && other < 0);
    }

    public static bool IsIdle(this float f)
    {
        return f == 0f;
    }
}