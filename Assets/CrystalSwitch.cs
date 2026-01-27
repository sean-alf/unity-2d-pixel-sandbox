using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class CrystalSwitch : MonoBehaviour
{
    public enum State
    {
        A,
        B,
    }

    [SerializeField] private Sprite aStateSprite;
    [SerializeField] private Sprite bStateSprite;
    [SerializeField] private State state;
    [SerializeField] private float totalRotation = 180f;

    public UnityEvent<State> onStateChange;
    public UnityEvent onStateA;
    public UnityEvent onStateB;

    [Space]
    [Header("Debug")]

    [SerializeField] private bool isChangingState = false;
    [SerializeField] private float remainingAngle;
    [SerializeField] private bool isLocked = false;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateSprite();

        remainingAngle = totalRotation;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            sr = GetComponent<SpriteRenderer>();
            UpdateSprite();
        }
        else
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                sr = GetComponent<SpriteRenderer>();
                UpdateSprite();
            };
        }
    }
#endif

    private void Update()
    {
        if (!isChangingState || isLocked) return;

        var step = Mathf.Min(1000 * Time.deltaTime, remainingAngle);
        transform.Rotate(new(0, 0, step));
        remainingAngle -= step;

        if (remainingAngle <= 0f)
        {
            isChangingState = false;
            transform.rotation = Quaternion.identity;
            remainingAngle = totalRotation;
            state = (State)(((int)state + 1) % Enum.GetValues(typeof(State)).Length);
            UpdateSprite();

            if (state == State.A) onStateA?.Invoke();
            if (state == State.B) onStateB?.Invoke();
            onStateChange?.Invoke(state);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isChangingState) return;
        isChangingState = true;
    }

    public void SetState(State state) => this.state = state;

    public void Lock() => isLocked = true;

    public void Unlock() => isLocked = false;

    private void UpdateSprite() => sr.sprite = state == State.A ? aStateSprite : bStateSprite;
}
