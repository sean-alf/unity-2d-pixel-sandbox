using System;
using UnityEngine;

public static class ComponentExtensions
{
    public static Component WhenFound<T>(this Component c, Action<T> fn) where T : Component
    {
        if (c.TryGetComponent(out T t))
        {
            fn?.Invoke(t);
        }
        return c;
    }

    public static Component WhenNotFound<T>(this Component c, Action fn) where T : Component
    {
        if (!c.TryGetComponent(out T t))
        {
            fn?.Invoke();
        }
        return c;
    }

    public static Component WhenSame<T>(this Component c, T other, Action<T> fn) where T : Component
    {
        if (c == other)
        {
            fn(other);
        }
        return c;
    }

    public static Component WhenNotSame<T>(this Component c, T other, Action<T> fn) where T : Component
    {
        if (c != other)
        {
            fn(other);
        }
        return c;
    }
}
