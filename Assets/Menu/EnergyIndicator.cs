using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class EnergyIndicator : MonoBehaviour
{
    private static readonly int PIXEL_PER_LEVEL_UNIT = 2;

    [SerializeField]
    private Image borderSR;

    [SerializeField]
    private Image backgroundSR;

    [SerializeField]
    private Image levelSR;

    [SerializeField]
    private Sprite borderSprite;

    [SerializeField]
    private Sprite brokenBorderSprite;

    private int max;
    private int level;

    [SerializeField]
    private Color okColor;

    [SerializeField]
    private Color halfEmptyColor;

    [SerializeField]
    private Color warningColor;

    [SerializeField]
    private Color dangerColor;

    public void SetMaximum(int max)
    {
        this.max = max;
        UpdateIcons();
    }

    public void SetMaximumAndFill(int max)
    {
        this.max = max;
        level = max;
        UpdateIcons();
    }

    public void IncreaseMaximum(int amount)
    {
        max += amount;
        UpdateIcons();
    }

    public void SetLevel(int level)
    {
        this.level = level;
        if (this.level > max) this.level = max;
        UpdateIcons();
    }

    public void IncreaseLevel(int amount)
    {
        level += amount;
        if (level > max) level = max;
        UpdateIcons();
    }

    public void DecreaseLevel(int amount)
    {
        level -= amount;
        if (level < 0) level = 0;
        UpdateIcons();
    }

    public void FillLevel()
    {
        level = max;
        UpdateIcons();
    }

    public void Listener_OnPlayerHealthChange(HealthManager.EventData data)
    {
        switch (data.type)
        {
            case HealthManager.EventType.Init:
                {
                    SetMaximum(data.maxHealth);
                    SetLevel(data.currentHealth);
                    break;
                }
            case HealthManager.EventType.Heal:
                {
                    // TODO: add heal feedback
                    SetLevel(data.currentHealth);
                    break;
                }
            case HealthManager.EventType.Damage:
                {
                    // TODO: add damage feedback
                    SetLevel(data.currentHealth);
                    break;
                }
            case HealthManager.EventType.Dead:
                {
                    // TODO: do stuff on dead
                    break;
                }
            case HealthManager.EventType.EditorUpdate:
                {
                    SetMaximum(data.maxHealth);
                    SetLevel(data.currentHealth);
                    break;
                }
        }
    }

    private void UpdateIcons()
    {
        if (borderSR == null || backgroundSR == null || levelSR == null) return;

        int levelWidth = level * PIXEL_PER_LEVEL_UNIT;
        int backgroundWidth = (max * PIXEL_PER_LEVEL_UNIT) + 2;
        int borderWidth = backgroundWidth + 2;
        borderSR.rectTransform.sizeDelta = new(borderWidth, borderSR.rectTransform.sizeDelta.y);
        backgroundSR.rectTransform.sizeDelta = new(backgroundWidth, backgroundSR.rectTransform.sizeDelta.y);
        levelSR.rectTransform.sizeDelta = new(levelWidth, levelSR.rectTransform.sizeDelta.y);

        if (level > 0)
        {
            borderSR.sprite = borderSprite;
        }
        else
        {
            borderSR.sprite = brokenBorderSprite;
        }

        Color c = okColor;
        int levelPercent = (int)(level / (float)max * 100.0f);

        if (levelPercent <= 10)
        {
            c = dangerColor;
        }
        else if (levelPercent <= 25)
        {
            c = warningColor;
        }
        else if (levelPercent <= 50)
        {
            c = halfEmptyColor;
        }

        levelSR.color = c;
    }
}
