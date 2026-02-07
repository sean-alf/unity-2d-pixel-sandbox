using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(IAutoMoverTarget))]
[RequireComponent(typeof(Rigidbody2D))]
public class AutoMover : MonoBehaviour, ILoggerProvider
{
    public interface IAutoMoverTarget
    {
        public float Speed
        {
            get;
        }

        public void OnDirectionChanged(Vector2 direction);
    }

    private Rigidbody2D rb;

    [Space()]
    [Header("Debug")]

    [SerializeField]
    private Logger logger;

    private IAutoMoverTarget autoMoverTarget;

    public Logger Logger => logger;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        autoMoverTarget = GetComponent<IAutoMoverTarget>();
    }

    public void MoveTo(Vector2 position, Action onDone)
    {
        StartCoroutine(AutoMoveTo(position, Vector2.zero, onDone));
    }

    public void MoveTo(Vector2 position, Vector2 finishingDirection, Action onDone)
    {
        StartCoroutine(AutoMoveTo(position, finishingDirection, onDone));
    }

    private IEnumerator AutoMoveTo(Vector2 position, Vector2 finishingDirection, Action onDone)
    {
        float speed = autoMoverTarget.Speed;
        Vector2 xTarget = new(position.x, rb.position.y);
        Vector2 direction = transform.position.Direction(position);
        Vector2 directionX = direction.DirectionX();
        Vector2 directionY = direction.DirectionY();

        while (rb.position != xTarget)
        {
            rb.MoveTowards(xTarget, speed * Time.fixedDeltaTime / 2.0f);

            logger.D($"self position {rb.position}");
            logger.D($"other position {position}");

            if (rb.DistanceX(position) > 0.5)
            {
                autoMoverTarget.OnDirectionChanged(directionX);
            }

            yield return new WaitForFixedUpdate();
        }

        while (rb.position != position)
        {
            rb.MoveTowards(position, speed * Time.fixedDeltaTime / 2.0f);
            autoMoverTarget.OnDirectionChanged(directionY);
            yield return new WaitForFixedUpdate();
        }

        if (!finishingDirection.IsIdle()) autoMoverTarget.OnDirectionChanged(finishingDirection);

        onDone?.Invoke();
    }
}
