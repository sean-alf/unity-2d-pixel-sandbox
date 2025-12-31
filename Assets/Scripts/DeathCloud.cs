using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DeathCloud : MonoBehaviour
{
    public Action onAnimationEnd;

    public void Begin()
    {
        var animator = GetComponent<Animator>();
        animator.Play(DeathCloudAnimatorStates.BaseLayer.DEATH_CLOUD_POOF);
    }

    public void Animator_OnPoofEnd()
    {
        onAnimationEnd?.Invoke();
        Destroy(gameObject);
    }
}
