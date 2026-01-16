using UnityEngine;

[RequireComponent(typeof(LinearAnimator))]
public class ShockOrb : MonoBehaviour
{
    [SerializeField]
    private LinearAnimator animator;

    private void Awake()
    {
        animator = GetComponent<LinearAnimator>();
    }

    private void Start() {
        animator.Animate("Default");
    }
}
