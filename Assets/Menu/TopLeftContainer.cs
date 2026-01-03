using UnityEngine;

public class TopLeftContainer : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer primaryWeaponIconRenderer;

    [SerializeField]
    private SpriteRenderer primaryToolIconRenderer;

    [SerializeField]
    private EnergyIndicator energyIndicator;

    public void UpdatePrimaryWeaponIcon(Sprite icon)
    {
        primaryWeaponIconRenderer.sprite = icon;
    }

    public void UpdatePrimaryToolIcon(Sprite icon)
    {
        primaryToolIconRenderer.sprite = icon;
    }

    public EnergyIndicator GetEnergyIndicator() => energyIndicator;
}
