using System.Collections.Generic;
using UnityEngine;

public class ExternalForceReceiver : MonoBehaviour
{
    private readonly Dictionary<Component, Vector2> externalForces = new();

    public Vector2 AppliedForce => appliedForce;

    [Space]
    [Header("Debug")]

    [SerializeField]
    private Vector2 appliedForce;

    public void AddForce(Component c, Vector2 force)
    {
        appliedForce += force;
        externalForces.Add(c, force);
    }

    public void RemoveForce(Component c)
    {
        if (externalForces.ContainsKey(c))
        {
            appliedForce -= externalForces[c];
            externalForces.Remove(c);
        }
    }

    public void UpdateForce(Component c, Vector2 force)
    {
        RemoveForce(c);
        AddForce(c, force);
    }
}
