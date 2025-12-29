using UnityEngine;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteResolver))]
public class Door : MonoBehaviour
{
    [SerializeField]
    private bool startOpen = false;

    private Animator animator;
    private new BoxCollider2D collider;
    private SpriteResolver sr;
    private bool isOpen = false;
    private bool ignore = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();

        isOpen = startOpen;
    }

    private void Start()
    {
        animator.Play(startOpen ? "DoorOpened" : "DoorClosed");
    }

    public void Open()
    {
        if (ignore || isOpen) return;
        animator.Play("DoorOpen");
    }

    public void Close()
    {
        if (ignore || !isOpen) return;
        animator.Play("DoorClose");
    }

    public void Toggle()
    {
        if (ignore) return;

        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Animator_OnOpening()
    {
        ignore = true;
    }

    public void Animator_OnOpened()
    {
        isOpen = true;
        collider.enabled = false;
        ignore = false;
    }

    public void Animator_OnClosing()
    {
        ignore = true;
        collider.enabled = true;
    }

    public void Animator_OnClosed()
    {
        isOpen = false;
        ignore = false;
    }

    private void OnValidate()
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteResolver>();
        }

        if (startOpen)
        {
            sr.SetCategoryAndLabel("Default", "Open");
        }
        else
        {
            sr.SetCategoryAndLabel("Default", "Closed");
        }

        sr.ResolveSpriteToSpriteRenderer();
    }
}
