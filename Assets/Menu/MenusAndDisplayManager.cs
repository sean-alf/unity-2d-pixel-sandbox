using UnityEngine;

public class MenusAndDisplayManager : MonoBehaviour, ILogTagProvider
{
    [SerializeField]
    private TopLeftContainer topLeftContainer;

    [Header("Debug")]
    [Space]
    
    [SerializeField]
    private LogLevelSelector logLevelSelector;

    private Logging.Tag logTag;

    public Logging.Tag LogTag => logTag;

    private void OnEnable()
    {
        logTag = this.CreateLogTag();
        Logging.SetLogLevel(logTag, logLevelSelector.logLevel);
    }

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
        Logging.LogInfo(logTag, $"activate {!topLeftContainer.gameObject.activeSelf}");
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
