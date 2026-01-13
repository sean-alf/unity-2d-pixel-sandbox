using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct ColorByStrength
{
    public Color color;
    public int greaterOrEqualToStrength;
}

public class HealthPickup : MonoBehaviour
{
    [Tooltip("Do not modify in the Inspector. Updated by ST2U prefab replacer")]
    public int healAmount = 5;

    public int HealAmount => healAmount;

    [SerializeField]
    [Tooltip("Do not modify in the Inspector. Exposed for debugging purposes only")]
    private Color currentColor;

    [SerializeField]
    private List<ColorByStrength> colorByStrength;

    private void Awake()
    {
        UpdateColorBasedOnHealAmount();
    }

    private void OnValidate()
    {
        UpdateColorBasedOnHealAmount();
    }

    private void UpdateColorBasedOnHealAmount()
    {
        var child = transform.GetChild(0);

        if (colorByStrength.Count > 0 && child.TryGetComponent(out SpriteRenderer sr))
        {
            var c = colorByStrength.OrderByDescending(c => c.greaterOrEqualToStrength)
                .First(c => c.greaterOrEqualToStrength <= healAmount);
            sr.color = c.color;
            currentColor = c.color;
        }
    }
}
