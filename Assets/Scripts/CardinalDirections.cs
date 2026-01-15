using UnityEngine;

public enum CardinalDirections
{
    Up,
    Right,
    Down,
    Left
}

public static class CardinalDirectionsExtensions
{
    public static Vector2 ToVector2(this CardinalDirections c) => c switch
    {
        CardinalDirections.Up => Vector2.up,
        CardinalDirections.Right => Vector2.right,
        CardinalDirections.Down => Vector2.down,
        CardinalDirections.Left => Vector2.left,
        _ => Vector2.zero
    };
}
