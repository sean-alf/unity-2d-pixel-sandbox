using UnityEngine;

public class PixelMovement2D
{
    private static readonly string TAG = "PixelMovement2D";
    private static readonly ILogger logger = Debug.unityLogger;

    private Vector2 positionAccumulator = new(0, 0);
    private Vector2 prevAccumulator = new(0, 0);
    private Vector2 currentRoundedInput = new(0, 0);
    private Vector2 prevRoundedInput = new(0, 0);

    private readonly Rigidbody2D rb;

    public bool enableLogs;

    public PixelMovement2D(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    /// <summary>
    /// The value of direction does not need to be normalized and/or rounded before passing into this function.
    /// This function ignores any blocked direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void Move(Vector2 direction, float speed)
    {
        Move(direction, new(0, 0), speed);
    }

    /// <summary>
    /// The value of direction MUST BE normalized and rounded before passing into this function.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="blocked"></param>
    /// <param name="speed"></param>
    // public void Move(Vector2 direction, Vector2 blocked, float speed)
    // {
    //     // Don't do anything if idle
    //     if (direction.IsIdle()) return;

    //     currentRoundedInput = direction;

    //     if (enableLogs && currentRoundedInput != prevRoundedInput)
    //     {
    //         prevRoundedInput = currentRoundedInput;
    //         logger.Log(TAG, $"input {currentRoundedInput}, speed {speed}");
    //     }

    //     if (blocked.IsSameXDirection(direction))
    //     {
    //         positionAccumulator.x = 0;
    //     }
    //     else if (!currentRoundedInput.IsXIdle())
    //     {
    //         // positionAccumulator.x += speed * Time.fixedDeltaTime * roundedInput.x;
    //         positionAccumulator.x += speed * currentRoundedInput.x;
    //     }

    //     if (blocked.IsSameYDirection(direction))
    //     {
    //         positionAccumulator.y = 0;
    //     }
    //     else if (!currentRoundedInput.IsYIdle())
    //     {
    //         positionAccumulator.y += speed * currentRoundedInput.y;
    //     }

    //     if (enableLogs && positionAccumulator != prevAccumulator)
    //     {
    //         prevAccumulator = positionAccumulator;
    //         logger.Log(TAG, $"pos acc {positionAccumulator}");
    //     }

    //     Vector2 newPosition = rb.position.Add(positionAccumulator).Truncate();

    //     if (enableLogs && rb.position != newPosition)
    //     {
    //         logger.Log(TAG, $"rb pos {rb.position}, new pos {newPosition}, pos acc {positionAccumulator}");
    //     }

    //     rb.MovePosition(newPosition);

    //     // Reset the accumulator if it is large enough to cause movement
    //     if (Mathf.Abs(positionAccumulator.x) >= 1)
    //     {
    //         positionAccumulator.x = 0;
    //     }

    //     if (Mathf.Abs(positionAccumulator.y) >= 1)
    //     {
    //         positionAccumulator.y = 0;
    //     }
    // }

    public void Move(Vector2 direction, Vector2 blocked, float speed)
    {
        rb.MovePosition((Time.fixedDeltaTime * speed * direction) + rb.position);
    }

    public void DrawGizmos()
    {
        Gizmos.DrawSphere(new(positionAccumulator.x, positionAccumulator.y, 1), 2);
    }
}
