using UnityEngine;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RemoteInteractable))]
public class Door : MonoBehaviour
{
    // Exposed for ST2U prefab replacer
    public bool startOpen = false;
    public bool vertical = false;
    public bool reversible = false; // Door can be re-opened once closed, and re-closed once opened

    private Animator animator;
    private new BoxCollider2D collider;
    private RemoteInteractable interactable;
    private bool isOpen = false;
    private bool ignore = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();
        interactable = GetComponent<RemoteInteractable>();

        isOpen = startOpen;

        SetInitialState();
    }

    private void OnEnable()
    {
        interactable.onInteract += Interact;
    }

    private void OnDisable()
    {
        interactable.onInteract -= Interact;
    }

    private void Start()
    {
        animator.Play(startOpen ? DoorAnimatorStates.BaseLayer.DOOR_OPENED : DoorAnimatorStates.BaseLayer.DOOR_CLOSED);
    }

    public void Open()
    {
        if (ignore || isOpen) return;
        animator.Play(DoorAnimatorStates.BaseLayer.DOOR_OPEN);
    }

    public void Close()
    {
        if (ignore || !isOpen) return;
        animator.Play(DoorAnimatorStates.BaseLayer.DOOR_CLOSE);
    }

    public void Toggle()
    {
        if (ignore) return;
        if (isOpen) Close(); else Open();
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
        if (TryGetComponent(out SpriteResolver sr))
        {
            sr.SetCategoryAndLabel("Default", startOpen ? "Open" : "Closed");
            sr.ResolveSpriteToSpriteRenderer();
        }

        SetInitialState();
    }

    private void SetInitialState()
    {
        if (vertical && TryGetComponent(out SpriteRenderer s))
        {
            transform.rotation = Quaternion.Euler(new(0, 0, 90));
            s.flipX = true;
        }
    }

    public void Interact()
    {
        if (reversible || startOpen == isOpen)
        {
            Toggle();
        }
    }
}
