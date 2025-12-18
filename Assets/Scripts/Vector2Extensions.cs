using System;
using UnityEngine;

public static class Vector2Extensions
{
    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's y value represents up</returns>
    public static bool IsUp(this Vector2 v)
    {
        return v.y > 0;
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's y value represents down</returns>
    public static bool IsDown(this Vector2 v)
    {
        return v.y < 0;
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's y value represents up or down</returns>
    public static bool IsVertical(this Vector2 v)
    {
        return v.y != 0;
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's y value is 0</returns>
    public static bool IsYIdle(this Vector2 v)
    {
        return v.y.IsIdle();
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's x value represents left</returns>
    public static bool IsLeft(this Vector2 v)
    {
        return v.x < 0;
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's x value represents right</returns>
    public static bool IsRight(this Vector2 v)
    {
        return v.x > 0;
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's x value represents left or right</returns>
    public static bool IsHorizontal(this Vector2 v)
    {
        return v.x != 0;
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's x value is 0</returns>
    public static bool IsXIdle(this Vector2 v)
    {
        return v.x.IsIdle();
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's x and y values are both 0</returns>
    public static bool IsIdle(this Vector2 v)
    {
        return v.IsXIdle() && v.IsYIdle();
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's and the passed in vector's x directions are the same, ignoring magnitude</returns>
    public static bool IsSameXDirection(this Vector2 v, Vector2 other)
    {
        return v.x.IsSameDirection(other.x);
    }

    /// <summary>
    /// Expects the vector to be normalized.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if this vector's and the passed in vector's y directions are the same, ignoring magnitude</returns>
    public static bool IsSameYDirection(this Vector2 v, Vector2 other)
    {
        return v.y.IsSameDirection(other.y);
    }

    /// <summary>
    /// Returns a Vector2 ensuring only a single axis is set.
    /// If both axes are set then the preferred axis will be chosen (X axis if preferX is true, otherwise Y axis).
    /// If only a single axis is set (or none at all) then the original Vector2 is returned.
    /// This is to prevent diagonal direction vectors.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="preferX"></param>
    /// <returns></returns>
    public static Vector2 ToSingleAxis(this Vector2 v, bool preferX)
    {
        if (v.IsHorizontal() && v.IsVertical())
        {
            return preferX ? new(v.x, 0) : new(0, v.y);
        }

        return v;
    }

    /// <summary>
    /// Returns a new Vector2 with rounded values based on this Vector2.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector2 Round(this Vector2 v)
    {
        return new(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }

    /// <summary>
    /// Clamps the Vector2 x and y values towards 0.<br/>
    /// Example:
    /// <br/><br/>
    ///     • x is 1.25 (positive), so x will be clamped to 1<br/>
    ///     • y is -5.75 (negative), so y will be clamped to -5
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector2 Truncate(this Vector2 v)
    {
        return new((float)Math.Truncate(v.x), (float)Math.Truncate(v.y));
    }

    /// <summary>
    /// Returns a new Vector2 with noramzlied and rounded values based on this Vector2.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector2 NormalizeAndRound(this Vector2 v)
    {
        return v.normalized.Round();
    }

    public static Vector2 Add(this Vector2 v, Vector2 other)
    {
        return v + other;
    }
}