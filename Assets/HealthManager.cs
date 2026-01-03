using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField]
    private int maximumHealth;

    [SerializeField]
    private int currentHealth;

    [SerializeField]
    private MenusAndDisplayManager menusAndDisplayManager;

    private EnergyIndicator energyIndicator;

    private void Awake()
    {
        currentHealth = maximumHealth;
        energyIndicator = menusAndDisplayManager.GetTopLeftContainer().GetEnergyIndicator();
        energyIndicator.SetMaximumAndFill(maximumHealth);
    }

    public void DoDamage(int strength)
    {
        currentHealth -= strength;

        energyIndicator.DecreaseLevel(strength);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("I got dead again!!");
        gameObject.SetActive(false);
    }
}
