using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LinearAnimator))]
public class RemovableBarrier : MonoBehaviour
{
    public UnityEvent onRemoved;
    public UnityEvent onPutBack;

    private Sprite sprite;
    private SpriteRenderer sr;
    private new Collider2D collider;
    private LinearAnimator animator;
    private float width;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<LinearAnimator>();

        sprite = sr.sprite;
        width = sr.size.x;
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

    public void PutBack()
    {
        sr.sprite = sprite;
        sr.size = new(width, sr.size.y);
        collider.enabled = true;
        onPutBack?.Invoke();
    }
}
