using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Switch : MonoBehaviour
{
    private static readonly string AnimationKey = "Default";

    [SerializeField]
    private Position switchPosition;

    [SerializeField]
    private bool isLocked = false;

    [SerializeField]
    private Position[] playerCanToggleFrom;

    [SerializeField]
    private PositionSprite[] positionSprites;

    [SerializeField]
    private SwitchEvent[] events;

    [SerializeField]
    private UnityEvent onToggleImmediateEvent;

    private LinearAnimator animator;

    private void Awake()
    {
        animator = GetComponent<LinearAnimator>();
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

    public void Toggle() => ToggleInternal(false);

    public void Interactable_Toggle() => ToggleInternal(true);

    public void Lock() => isLocked = true;

    public void Unlock() => isLocked = false;

    private void Init()
    {
        var ps = positionSprites.FirstOrDefault(ps => ps.forPosition == switchPosition);
        if (ps.sprite) GetComponent<SpriteRenderer>().sprite = ps.sprite;
    }

    private void ToggleInternal(bool isPlayer)
    {
        if (isLocked) return;

        if (!isPlayer || playerCanToggleFrom.Contains(switchPosition))
        {
            isLocked = true;
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
        UpdatePosition();
        isLocked = false;
        foreach (var e in events.Where(e => e.forPosition == switchPosition)) e.whenSelected?.Invoke();
    }

    private void UpdatePosition()
    {
        int pos = (int)switchPosition;
        pos = (pos + 1) % Enum.GetValues(typeof(Position)).Length;
        switchPosition = (Position)pos;
    }

    public enum Position
    {
        A,
        B,
    }

    [Serializable]
    public struct SwitchEvent
    {
        public Position forPosition;
        public UnityEvent whenSelected;
    }

    [Serializable]
    public struct PositionSprite
    {
        public Position forPosition;
        public Sprite sprite;
    }
}
