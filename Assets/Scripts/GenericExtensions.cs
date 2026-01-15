using System;

public static class GenericExtensions
{
    public static T WhenNull<T>(this T t, Action onNull) where T : UnityEngine.Object
    {
        if (t == null) onNull();
        return t;
    }

    public static T WhenNullClass<T>(this T t, Action onNull) where T : class
    {
        if (t == null) onNull();
        return t;
    }

    public static T WhenNotNullClass<T>(this T t, Action<T> onNotNull) where T : class
    {
        if (t != null) onNotNull(t);
        return t;
    }

    public static T WhenNotNull<T>(this T t, Action<T> onNotNull) where T : UnityEngine.Object
    {
        if (t != null) onNotNull(t);
        return t;
    }
}
