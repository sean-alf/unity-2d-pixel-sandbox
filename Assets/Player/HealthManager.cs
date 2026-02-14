using System;
using UnityEditor;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField][Range(10, 50)] private int maximumHealth;
    [SerializeField][Range(0, 50)] private int currentHealth;
    [SerializeField] private HealthChangeEvent healthChangeEvent;
    // Allows disabling of damage while still acting like it's being received
    [SerializeField] private bool enableDamage = true;

    [Space]
    [Header("Debug")]
    // In-game invincibility
    public bool invincible = false;

    public bool IsDead => currentHealth <= 0;

    public enum EventType
    {
        Init,
        Heal,
        Damage,
        Dead,
        EditorUpdate
    }

    public readonly struct EventData
    {
        public readonly EventType type;
        public readonly int currentHealth;
        public readonly int maxHealth;

        public EventData(EventType type, int currentHealth, int maxHealth)
        {
            this.type = type;
            this.currentHealth = currentHealth;
            this.maxHealth = maxHealth;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // GameObject Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    private void Start()
    {
        healthChangeEvent.Raise(new(
            type: EventType.Init,
            currentHealth,
            maximumHealth
        ));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);

        EditorApplication.delayCall += () =>
        {
            healthChangeEvent.Raise(new(
                type: EventType.EditorUpdate,
                currentHealth,
                maximumHealth)
            );
        };
    }
#endif

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Decrease health by "strength" amount.
    /// </summary>
    /// <param name="strength"></param>
    /// <returns>True if damage has been recieved</returns>
    public bool DoDamage(int strength)
    {
        if (invincible) return false;

        if (enableDamage)
        {
            currentHealth = Mathf.Max(currentHealth - strength, 0);
            healthChangeEvent.Raise(new(
                type: EventType.Damage,
                 currentHealth,
                 maximumHealth
            ));
            if (IsDead) Die();
        }

        return true;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maximumHealth);
        healthChangeEvent.Raise(new(
            type: EventType.Heal,
             currentHealth,
             maximumHealth
        ));
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private void Die()
    {
        Debug.Log("I got dead again!!");
        healthChangeEvent.Raise(new(
            type: EventType.Dead,
            currentHealth,
            maximumHealth
        ));
        gameObject.SetActive(false);
    }
}
