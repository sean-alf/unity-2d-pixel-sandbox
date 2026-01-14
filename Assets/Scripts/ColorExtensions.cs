using UnityEngine;

public static class ColorExtensions
{
    public static Color WithRed(this Color c, float r) => new(r, c.g, c.b, c.a);
    public static Color WithGreen(this Color c, float g) => new(c.r, g, c.b, c.a);
    public static Color WithBlue(this Color c, float b) => new(c.r, c.g, b, c.a);
    public static Color WithAlpha(this Color c, float a) => new(c.r, c.g, c.b, a);
}
