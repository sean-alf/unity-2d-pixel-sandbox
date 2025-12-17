using System;
using UnityEngine;

[Serializable]
public class AnimationData
{
    private static readonly string TAG = "[AnimationDataSO]";
    private static readonly ILogger logger = Debug.unityLogger;

    [SerializeField]
    [Tooltip("The name of the Animator Controller animation state")]
    private string stateName;
    [SerializeField]
    [Tooltip("The name of the Animator Controller idle state")]
    private string idleStateName;
    [SerializeField]
    [Tooltip("The name of the Animator Controller attack state")]
    private string attackStateName;

    public string StateName
    {
        get
        {
            return stateName;
        }
    }

    public string IdleStateName
    {
        get
        {
            return idleStateName;
        }
    }

    public void Attack(Animator animator)
    {
        if (animator == null)
        {
            logger.LogError(TAG, "animator is null");
            return;
        }

        if (attackStateName == null || attackStateName.Length == 0)
        {
            logger.LogWarning(TAG, "attack state missing");
            return;
        }

        animator.Play(attackStateName);
    }
}
