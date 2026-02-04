using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LinearAnimator))]
public class RemovableBarrier : MonoBehaviour
{
    public UnityEvent onRemoved;

    private SpriteRenderer sr;
    private new Collider2D collider;
    private LinearAnimator animator;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<LinearAnimator>();
    }

    public void Remove()
    {
        animator.AnimateRealtime("Remove", onFinished: () =>
        {
            animator.Stop(clearSprite: true);
            onRemoved?.Invoke();
        });
        sr.sprite = null;
        collider.enabled = false;
    }
}
