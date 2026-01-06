using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField]
    [Range(10, 50)]
    private int maximumHealth;
    private int prevMaxHealth;

    [SerializeField]
    [Range(0, 50)]
    private int currentHealth;
    private int prevCurrentHealth;

    [SerializeField]
    private MenusAndDisplayManager menusAndDisplayManager;

    [SerializeField]
    private bool autoUpdateHealthIndicator = false;

    private EnergyIndicator energyIndicator;

    private void Awake()
    {
        prevMaxHealth = maximumHealth;
        currentHealth = maximumHealth;
        prevCurrentHealth = currentHealth;
    }

    private void Start()
    {
        if (energyIndicator == null)
        {
            energyIndicator = menusAndDisplayManager.GetTopLeftContainer().GetEnergyIndicator();
        }

        energyIndicator.SetMaximumAndFill(maximumHealth);
    }

    public void DoDamage(int strength)
    {
        currentHealth -= strength;
        prevCurrentHealth = currentHealth;

        energyIndicator.DecreaseLevel(strength);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(int amount)
    {
        currentHealth += amount;
        prevCurrentHealth = currentHealth;
        energyIndicator.IncreaseLevel(amount);
    }

    private void Die()
    {
        Debug.Log("I got dead again!!");
        gameObject.SetActive(false);
    }

    private void OnValidate()
    {
        if (!autoUpdateHealthIndicator) return;

        if (currentHealth > maximumHealth) currentHealth = maximumHealth;

        if (menusAndDisplayManager == null) return;

        if (energyIndicator == null)
        {
            energyIndicator = menusAndDisplayManager.GetTopLeftContainer().GetEnergyIndicator();
        }

        if (currentHealth != prevCurrentHealth)
        {
            prevCurrentHealth = currentHealth;
            energyIndicator.SetLevel(currentHealth);
        }

        if (maximumHealth != prevMaxHealth)
        {
            prevMaxHealth = maximumHealth;
            energyIndicator.SetMaximum(maximumHealth);
        }
    }
}
