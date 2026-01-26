using System;
using UnityEngine;
using UnityEngine.Events;

public class AccumulationEvent : MonoBehaviour
{
    [SerializeField] private int triggerCount = 1;

    public UnityEvent onTrigger;
    public UnityEvent onUntrigger;

    private int count = 0;

    public void Add()
    {
        if (count == triggerCount) return;
        if (++count == triggerCount) onTrigger?.Invoke();
    }

    public void Subtract()
    {
        var prevCount = count;
        count = Math.Max(--count, 0);
        if (prevCount == triggerCount && count < triggerCount) onUntrigger?.Invoke();
    }

    public void Reset() => count = 0;
}
