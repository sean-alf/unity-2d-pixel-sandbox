using System;
using UnityEngine;

public class KnockbackReceiver : MonoBehaviour
{
    public Vector2 KnockbackVelocity => knockbackVelocity;

    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float baseKnockbackFactor = 5f;
    [SerializeField] private AnimationCurve decayCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Space]
    [Header("Debug")]
    [SerializeField] private Vector2 knockbackVelocity;
    [SerializeField] private Vector2 initialVelocity;
    [SerializeField] private float timerCounter;
    [SerializeField] private bool knockbackEnabled = true;

    private Action onKnockBackDone;

    /// <summary>
    /// Starts the Knockback sequence.
    /// </summary>
    /// <param name="strength">Strength of the knockback, affects knockback distance</param>
    /// <param name="collision">The collision from which initiates the knockback sequence</param>
    /// <param name="onDone">Called when the knockback sequence is finished</param>
    /// <returns>True if the knockback was recieved</returns>
    public bool KnockBack(float strength, Collision2D collision, Action onDone)
    {
        if (onKnockBackDone != null || collision.contactCount == 0 || !knockbackEnabled) return false;

        var direction = transform.position
            .ToVector2()
            .Subtract(collision.contacts[0].point)
            .normalized;

        onKnockBackDone = onDone;
        timerCounter = duration;
        initialVelocity = (baseKnockbackFactor + strength) * direction;
        knockbackVelocity = initialVelocity;
        return true;
    }

    private void FixedUpdate()
    {
        if (timerCounter <= 0f) return;

        timerCounter -= Time.fixedDeltaTime;

        var t = 1f - (timerCounter / duration);
        var decay = decayCurve.Evaluate(t);
        knockbackVelocity = initialVelocity * decay;

        if (timerCounter <= 0f)
        {
            timerCounter = 0f;
            knockbackVelocity = Vector2.zero;
            onKnockBackDone();
            onKnockBackDone = null;
        }
    }
}
