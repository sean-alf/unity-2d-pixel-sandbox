using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ObjectExtensions
{
    public static Object IfNotNull<T>(this T o, Action<T> onNotNull) where T : Object
    {
        if (o != null) onNotNull(o);
        return o;
    }

    public static Object IfNull<T>(this T o, Action onNull) where T : Object
    {
        if (o == null) onNull();
        return o;
    }
}
