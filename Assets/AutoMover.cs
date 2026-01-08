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
        StartCoroutine(AutoMoveTo(position, onDone));
    }

    private IEnumerator AutoMoveTo(Vector2 position, Action onDone)
    {
        float factor = autoMoverTarget.Speed;
        Vector2 xTarget = new(position.x, rb.position.y);
        Vector2 direction = transform.position.Direction(position);
        Vector2 directionX = direction.DirectionX();
        Vector2 directionY = direction.DirectionY();

        logger.D($"self position {transform.position}");
        logger.D($"other position {position}");

        while (rb.position != xTarget)
        {
            rb.MoveTowards(xTarget, factor);

            if (rb.DistanceX(position) > 16)
            {
                autoMoverTarget.OnDirectionChanged(directionX);
            }

            yield return new WaitForFixedUpdate();
        }

        while (rb.position != position)
        {
            rb.MoveTowards(position, factor);
            autoMoverTarget.OnDirectionChanged(directionY);
            yield return new WaitForFixedUpdate();
        }

        autoMoverTarget.OnDirectionChanged(Vector2.zero);

        onDone?.Invoke();
    }
}
