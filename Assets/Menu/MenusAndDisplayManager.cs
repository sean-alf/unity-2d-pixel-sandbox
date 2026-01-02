using UnityEngine;

public class MenusAndDisplayManager : MonoBehaviour
{
    [SerializeField]
    private TopLeftContainer topLeftContainer;

    public void ShowTopLeftDisplay()
    {
        if (topLeftContainer.gameObject.activeSelf) return;
        topLeftContainer.gameObject.SetActive(true);
    }

    public void HideTopLeftDisplay()
    {
        if (!topLeftContainer.gameObject.activeSelf) return;
        topLeftContainer.gameObject.SetActive(false);
    }

    public void ToggleTopLeftDisplay()
    {
        Debug.Log($"MADM: activate {!topLeftContainer.gameObject.activeSelf}");
        topLeftContainer.gameObject.SetActive(!topLeftContainer.gameObject.activeSelf);
    }

    public void UpdatePrimaryWeaponIcon(Sprite icon)
    {
        topLeftContainer.UpdatePrimaryWeaponIcon(icon);
    }

    public void UpdatePrimaryToolIcon(Sprite icon)
    {
        topLeftContainer.UpdatePrimaryToolIcon(icon);
    }
}
