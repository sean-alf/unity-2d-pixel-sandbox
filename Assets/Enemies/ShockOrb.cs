using UnityEngine;

[RequireComponent(typeof(LinearAnimator))]
public class ShockOrb : MonoBehaviour
{
    [SerializeField]
    private LinearAnimator animator;

    private float rotationFactor = 1;

    private void Awake()
    {
        animator = GetComponent<LinearAnimator>();
    }

    private void Start()
    {
        animator.Animate("Default");
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, 1), 360 * rotationFactor * Time.deltaTime);
    }

    public void StopRotation() => rotationFactor = 0;
}
