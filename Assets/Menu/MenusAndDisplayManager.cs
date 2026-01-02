using UnityEngine;

public class MenusAndDisplayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject topLeftDisplay;

    public void ShowTopLeftDisplay()
    {
        if (topLeftDisplay.activeSelf) return;
        topLeftDisplay.SetActive(true);
    }

    public void HideTopLeftDisplay()
    {
        if (!topLeftDisplay.activeSelf) return;
        topLeftDisplay.SetActive(false);
    }

    public void ToggleTopLeftDisplay()
    {
        Debug.Log($"MADM: activate {!topLeftDisplay.activeSelf}");
        topLeftDisplay.SetActive(!topLeftDisplay.activeSelf);
    }
}
