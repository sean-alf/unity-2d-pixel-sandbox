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

    private Action onKnockBackDone;

    public void KnockBack(float strength, Collision2D collision, Action onDone)
    {
        if (onKnockBackDone != null || collision.contactCount == 0) return;

        var direction = transform.position
            .ToVector2()
            .Subtract(collision.contacts[0].point)
            .normalized;

        onKnockBackDone = onDone;
        timerCounter = duration;
        initialVelocity = (baseKnockbackFactor + strength) * direction;
        knockbackVelocity = initialVelocity;
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
