using System;
using UnityEngine;

public static class NumberExtensions
{
    /// <summary>
    /// Increments the int.
    /// Returns true and calls onClamped when the value has been clamped.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="clamp"></param>
    /// <param name="onClamped"></param>
    /// <returns></returns>
    public static bool Increment(this ref int i, int clamp, Action onClamped = null)
    {
        if (++i > clamp)
        {
            i = clamp;
            onClamped?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Decrements the int.
    /// Returns true and calls onClamped when the value has been clamped.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="clamp"></param>
    /// <param name="onClamped"></param>
    /// <returns></returns>
    public static bool Decrement(this ref int i, int clamp, Action onClamped = null)
    {
        if (--i < clamp)
        {
            i = clamp;
            onClamped?.Invoke();
            return true;
        }

        return false;
    }
}
