using UnityEngine;

public class PixelMovement2D
{
    private static readonly string TAG = "PixelMovement2D";
    private static readonly ILogger logger = Debug.unityLogger;

    private Vector2 rbPositionTmp;
    private readonly Rigidbody2D rb;

    public bool enableLogs;

    public PixelMovement2D(Rigidbody2D rb)
    {
        this.rb = rb;
        rbPositionTmp = this.rb.position;
    }

    /// <summary>
    /// The value of direction does not need to be normalized and/or rounded before passing into this function.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void Move(Vector2 direction, float speed)
    {
        Move(direction, new(0, 0), speed);
    }

    /// <summary>
    /// The value of direction does not need to be normalized and/or rounded before passing into this function.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="blocked"></param>
    /// <param name="speed"></param>
    public void Move(Vector2 direction, Vector2 blocked, float speed)
    {
        // Don't do anything if idle
        if (direction.IsIdle()) return;

        // Rounding ensures movement speed is the same whether it's horizontal, vertical, or diagonal
        Vector2 roundedInput = new(
            blocked.x.IsSameDirection(direction.x) ? 0 : Mathf.Round(direction.normalized.x),
            blocked.y.IsSameDirection(direction.y) ? 0 : Mathf.Round(direction.normalized.y)
        );

        // If movement is blocked in the X direction, set the tmp to the rigidbody value so that the tmp
        // will not wander from the rigidbody's pivot point
        if (blocked.x.IsSameDirection(direction.x))
        {
            rbPositionTmp.x = rb.position.x;
        }
        else
        {
            rbPositionTmp.x += speed * Time.fixedDeltaTime * roundedInput.x;
        }

        // If movement is blocked in the Y direction, set the tmp to the rigidbody value so that the tmp
        // will not wander from the rigidbody's pivot point
        if (blocked.y.IsSameDirection(direction.y))
        {
            rbPositionTmp.y = rb.position.y;
        }
        else
        {
            rbPositionTmp.y += speed * Time.fixedDeltaTime * roundedInput.y;
        }

        Vector2 roundedPosition = new(
            Mathf.Round(rbPositionTmp.x),
            Mathf.Round(rbPositionTmp.y)
        );

        if (enableLogs)
        {
            logger.Log(TAG, $@"Move
    rb pos {rb.position}
    rb pos tmp {rbPositionTmp}
    rounded pos {roundedPosition}
    rounded input {roundedInput}
    speed {speed}
    fixedTimeDelta {Time.fixedDeltaTime}");
        }

        rb.MovePosition(roundedPosition);
    }

    /// <summary>
    /// Resets the temporary position to the current RigidBody2D position, in case it had to be moved externally.
    /// </summary>
    public void ResetPosition()
    {
        rbPositionTmp = rb.position;
    }

    public void DrawGizmos()
    {
        Gizmos.DrawSphere(new(rbPositionTmp.x, rbPositionTmp.y, 1), 2);
    }
}
