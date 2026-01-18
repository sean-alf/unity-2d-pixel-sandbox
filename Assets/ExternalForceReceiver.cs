using UnityEngine;

public class ExternalForceReceiver : MonoBehaviour
{
    public Vector2 AppliedForce => appliedForce;

    [Space]
    [Header("Debug")]

    [SerializeField]
    private Vector2 appliedForce;

    public void AddForce(Vector2 force) => appliedForce += force;

    public void RemoveForce(Vector2 force) => appliedForce -= force;
}
