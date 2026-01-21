using UnityEngine;

public enum CardinalDirection
{
    Up,
    Right,
    Down,
    Left
}

public static class CardinalDirectionsExtensions
{
    public static bool IsCardinal(this Vector2 v) => v == Vector2.up ||
        v == Vector2.right ||
        v == Vector2.down ||
        v == Vector2.left;

    public static Vector2 ToVector2(this CardinalDirection c) => c switch
    {
        CardinalDirection.Up => Vector2.up,
        CardinalDirection.Right => Vector2.right,
        CardinalDirection.Down => Vector2.down,
        CardinalDirection.Left => Vector2.left,
        _ => Vector2.zero
    };

    public static Quaternion ToRotation(this CardinalDirection c) => c switch
    {
        CardinalDirection.Up => Quaternion.Euler(0, 0, 0),
        CardinalDirection.Right => Quaternion.Euler(0, 0, -90),
        CardinalDirection.Down => Quaternion.Euler(0, 0, 180),
        CardinalDirection.Left => Quaternion.Euler(0, 0, 90),
        _ => Quaternion.identity
    };
}
