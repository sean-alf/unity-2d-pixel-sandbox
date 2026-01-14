using UnityEngine;
using UnityEngine.UI;

public class TopLeftContainer : MonoBehaviour
{
    [SerializeField]
    private Image primaryWeaponIconImage;

    [SerializeField]
    private Image primaryToolIconImage;

    [SerializeField]
    private EnergyIndicator energyIndicator;

    public void UpdatePrimaryWeaponIcon(Sprite icon)
    {
        primaryWeaponIconImage.sprite = icon;
    }

    public void UpdatePrimaryToolIcon(Sprite icon)
    {
        primaryToolIconImage.sprite = icon;
    }

    public EnergyIndicator GetEnergyIndicator() => energyIndicator;
}
