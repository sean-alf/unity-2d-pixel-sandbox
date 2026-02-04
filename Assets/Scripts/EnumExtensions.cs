using System;

public static class EnumCache<E> where E : struct, Enum
{
    public static readonly int Length = Enum.GetValues(typeof(E)).Length;
}

public static class EnumExtensions
{
    public static E IncrementAndWrap<E>(this E e, int incBy = 1, E defaultValue = default) where E : struct, Enum
    {
        int val = Convert.ToInt32(e);
        val = (val + incBy) % EnumCache<E>.Length;

        if (Enum.IsDefined(typeof(E), val)) return (E)Enum.ToObject(typeof(E), val);
        return defaultValue;
    }

    public static E DecrementAndWrap<E>(this E e, int decBy = 1, E defaultValue = default) where E : Enum
    {
        int length = Enum.GetValues(typeof(E)).Length;
        int val = Convert.ToInt32(e);
        val = (val - decBy + length) % length;

        if (Enum.IsDefined(typeof(E), val)) return (E)Enum.ToObject(typeof(E), val);
        return defaultValue;
    }
}
