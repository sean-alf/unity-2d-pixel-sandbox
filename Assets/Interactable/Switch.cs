using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Interactable))]
public class Switch : MonoBehaviour, Interactable.IOverride
{
    public enum Orientation
    {
        Vertical,
        Horizontal,
    }

    private static readonly string AnimationKey = "Default";

    public UnityEvent onToggleImmediateEvent;

    [SerializeField] private Position switchPosition;
    [SerializeField] private Orientation orientation = Orientation.Vertical;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private Position[] playerCanToggleFrom;
    [SerializeField] private PositionSprite[] positionSprites;
    [SerializeField] private UnityEvent<Switch> positionAEvent = new();
    [SerializeField] private UnityEvent<Switch> positionBEvent = new();
    [SerializeField] private UnityEvent<Switch, Position> stateChangeEvent = new();

    [Space]
    [Header("Debug")]
    [SerializeField] private bool isAnimating = false;
    [SerializeField] private Interactable.IInteractor interactor;


    private LinearAnimator animator;
    private Interactable interactable;


    public bool IsInteractable => !isLocked && !isAnimating && playerCanToggleFrom.Contains(switchPosition);

    private void Awake()
    {
        animator = GetComponent<LinearAnimator>();
        interactable = GetComponent<Interactable>();
        Init();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Init();
        }
        else
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                Init();
            };
        }
    }
#endif

    /// <summary>
    /// Use when automatic player input enabling/disabling is NOT desired.
    /// Preferrable for use when complex sequencing (involving this switch) is required.
    /// </summary>
    public void Toggle()
    {
        // Setting this null will make it ignore automatic input enabling and disabling during the toggle animation
        interactor = null;
        ToggleInternal();
    }

    /// <summary>
    /// Use when automatic player input enabling/disable IS desired.
    /// Great for use with ordinary switches that are not involed in complex sequencing.
    /// </summary>
    /// <param name="interactor"></param>
    public void Interactable_Toggle(Interactable.IInteractor interactor)
    {
        // Setting this here will make it use automatic input enabling and disabling during the toggle animation
        this.interactor = interactor;
        ToggleInternal();
    }

    public void AddStateChangeListener(UnityAction<Switch, Position> a) => stateChangeEvent.AddListener(a);
    public void RemoveStateChangeListener(UnityAction<Switch, Position> a) => stateChangeEvent.RemoveListener(a);
    public void RemoveAllStateChangeListeners() => stateChangeEvent.RemoveAllListeners();

    public void AddListener(Position p, UnityAction<Switch> a)
    {
        switch (p)
        {
            case Position.A:
                positionAEvent.AddListener(a);
                break;
            case Position.B:
                positionBEvent.AddListener(a);
                break;
        }
    }

    public void RemoveListener(Position p, UnityAction<Switch> a)
    {
        switch (p)
        {
            case Position.A:
                positionAEvent.RemoveListener(a);
                break;
            case Position.B:
                positionBEvent.RemoveListener(a);
                break;
        }
    }

    public void RemoveAllListeners()
    {
        RemoveListeners(Position.A);
        RemoveListeners(Position.B);
    }

    public void RemoveListeners(Position p)
    {
        switch (p)
        {
            case Position.A:
                positionAEvent.RemoveAllListeners();
                break;
            case Position.B:
                positionBEvent.RemoveAllListeners();
                break;
        }
    }

    public void Lock()
    {
        if (isLocked) return;
        isLocked = true;
        interactable.NotifyStateChanged(gameObject);
    }

    public void Unlock()
    {
        if (!isLocked) return;
        isLocked = false;
        interactable.NotifyStateChanged(gameObject);
    }

    private void Init()
    {
        var ps = positionSprites.FirstOrDefault(ps => ps.forPosition == switchPosition);
        if (ps.sprite) GetComponent<SpriteRenderer>().sprite = ps.sprite;
        UpdateAppearanceBasedOnOrientation();
    }

    private void ToggleInternal()
    {
        if (isLocked) return;

        if (interactor == null || playerCanToggleFrom.Contains(switchPosition))
        {
            isAnimating = true;
            interactor.InputManager.AddInputChangeRequest(interactable);
            onToggleImmediateEvent?.Invoke();
            StartPositionBasedAnimation();
        }
    }

    private void StartPositionBasedAnimation()
    {
        if (switchPosition == Position.A)
        {
            animator.Animate(AnimationKey, onFinished: () => OnAnimationFinished());
        }
        else
        {
            animator.AnimateReverse(AnimationKey, onFinished: () => OnAnimationFinished());
        }
    }

    private void OnAnimationFinished()
    {
        int pos = ((int)switchPosition + 1) % Enum.GetValues(typeof(Position)).Length;
        switchPosition = (Position)pos;
        isAnimating = false;
        interactable.NotifyStateChanged(gameObject);
        InvokeEvents(switchPosition);
        interactor.InputManager.RemoveInputChangeRequest(interactable);
        interactor = null;
    }

    private void InvokeEvents(Position p)
    {
        switch (p)
        {
            case Position.A:
                positionAEvent?.Invoke(this);
                break;
            case Position.B:
                positionBEvent?.Invoke(this);
                break;
        }
        stateChangeEvent?.Invoke(this, p);
    }

    private void UpdateAppearanceBasedOnOrientation()
    {
        if (TryGetComponent(out SpriteRenderer sr))
        {
            switch (orientation)
            {
                case Orientation.Vertical:
                    sr.flipY = true;
                    transform.rotation = Quaternion.Euler(0, 0, -90f);
                    break;
                case Orientation.Horizontal:
                    sr.flipY = false;
                    transform.rotation = Quaternion.identity;
                    break;
            }
        }
    }

    public enum Position
    {
        A,
        B,
    }

    [Serializable]
    public struct PositionSprite
    {
        public Position forPosition;
        public Sprite sprite;
    }
}
