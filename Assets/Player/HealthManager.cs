using System;
using UnityEditor;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField][Range(10, 50)] private int maximumHealth;
    [SerializeField][Range(0, 50)] private int currentHealth;
    [SerializeField] private HealthChangeEvent healthChangeEvent;

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

    public void DoDamage(int strength)
    {
        currentHealth = Mathf.Max(currentHealth - strength, 0);
        healthChangeEvent.Raise(new(
            type: EventType.Damage,
             currentHealth,
             maximumHealth
        ));
        if (currentHealth <= 0) Die();
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
