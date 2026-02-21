using UnityEngine;

[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class AlarmLight : MonoBehaviour
{
    [SerializeField] private Sprite okSprite;
    [SerializeField] private Sprite warnSprite;
    [SerializeField] private Sprite alertSprite1;
    [SerializeField] private Sprite alertSprite2;

    [Space]
    [Header("Debug")]

    [SerializeField] private State currentState = State.Ok;

    private SpriteRenderer sr;
    private LinearAnimator linearAnimator;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        linearAnimator = GetComponent<LinearAnimator>();
    }

    public void TransitionToOk() => SetState(State.Ok);

    public void TransitionToWarn() => SetState(State.Warn);

    public void TransitionToAlert() => SetState(State.Alert);

    public void SetState(State newState)
    {
        linearAnimator.Stop();
        currentState = newState;

        switch (currentState)
        {
            case State.Ok:
                sr.sprite = okSprite;
                break;
            case State.Warn:
                sr.sprite = warnSprite;
                break;
            case State.Alert:
                linearAnimator.Animate("Default");
                break;
        }
    }

    public enum State
    {
        Ok,
        Warn,
        Alert,
    }
}
