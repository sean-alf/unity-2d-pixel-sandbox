using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int HealAmount => healAmount;

    [SerializeField]
    private int healAmount = 5;
}
