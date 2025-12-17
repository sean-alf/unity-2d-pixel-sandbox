using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class BasicAnimation : MonoBehaviour
{
    private static readonly string TAG = "BasicAnimation";
    private static readonly ILogger logger = Debug.unityLogger;

    [SerializeField]
    private AnimationData downAnimationData;
    [SerializeField]
    private AnimationData upAnimationData;
    [SerializeField]
    private AnimationData leftAnimationData;
    [SerializeField]
    private AnimationData rightAnimationData;

    [Header("Debug")]
    [SerializeField]
    private bool enableLogs = false;

    private PlayerMovement movement;
    private Animator animator;
    private AnimationData currentAnimationData;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        currentAnimationData = downAnimationData;

        movement.OnDirectionChange += OnDirectionChange;
        movement.OnButtonChange += OnButtonChange;
    }

    private void OnButtonChange(PlayerMovement.PlayerButtonData data)
    {
        if (currentAnimationData != null)
        {
            if (data.primaryActionActive)
            {
                currentAnimationData.Attack(animator);
            }
        }
    }

    private void OnDirectionChange(PlayerMovement.PlayerMovementData data)
    {
        if (enableLogs)
        {
            logger.Log(TAG, $"data={data}");
        }

        if ((data.state & PlayerMovement.State.PAUSED) != 0)
        {
            // Ignore direction change if paused
            return;
        }

        if (data.currentDirection.IsIdle())
        {
            if (currentAnimationData != null && currentAnimationData.IdleStateName.Length > 0)
            {
                animator.Play(currentAnimationData.IdleStateName);
            }
            else
            {
                logger.LogWarning(TAG, "missing animationData or idleStateName");
                animator.Play(downAnimationData.IdleStateName);
            }
        }
        else
        {
            if (data.currentDirection.IsLeft())
            {
                currentAnimationData = leftAnimationData;
            }
            else if (data.currentDirection.IsRight())
            {
                currentAnimationData = rightAnimationData;
            }
            else if (data.currentDirection.IsDown())
            {
                currentAnimationData = downAnimationData;
            }
            else if (data.currentDirection.IsUp())
            {
                currentAnimationData = upAnimationData;
            }

            if (currentAnimationData != null && currentAnimationData.StateName.Length > 0)
            {
                if (enableLogs)
                {
                    logger.Log(TAG, $"anim name {currentAnimationData.StateName}");
                }

                animator.Play(currentAnimationData.StateName);
            }
            else
            {
                logger.LogWarning(TAG, "missing animationData or stateName");
            }
        }
    }

    void OnDestroy()
    {
        movement.OnDirectionChange -= OnDirectionChange;
        movement.OnButtonChange -= OnButtonChange;
    }
}