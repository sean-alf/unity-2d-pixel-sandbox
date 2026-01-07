using System;
using UnityEngine;

[RequireComponent(typeof(LinearAnimator))]
public class TeleportingAnimator : MonoBehaviour
{
    private LinearAnimator animator;

    private void Awake()
    {
        animator = GetComponent<LinearAnimator>();
    }

    public void Animate(Action onCover, Action onDone)
    {
        animator.Animate(() =>
        {
            onCover?.Invoke();
            animator.AnimateReverse(() =>
            {
                onDone?.Invoke();
                Destroy(gameObject);
            });
        });
    }
}
