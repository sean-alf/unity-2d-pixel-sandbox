using UnityEngine;

public class EnergyIndicator : MonoBehaviour
{
    private static readonly int PIXEL_PER_LEVEL_UNIT = 2;

    [SerializeField]
    private SpriteRenderer borderSR;

    [SerializeField]
    private SpriteRenderer backgroundSR;

    [SerializeField]
    private SpriteRenderer levelSR;

    [SerializeField]
    private Sprite borderSprite;

    [SerializeField]
    private Sprite brokenBorderSprite;

    [SerializeField]
    [Range(10, 50)]
    private int max;

    [SerializeField]
    [Range(0, 50)]
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

    private void UpdateIcons()
    {
        if (borderSR == null || backgroundSR == null || levelSR == null) return;

        int levelWidth = level * PIXEL_PER_LEVEL_UNIT;
        int maxWidth = (max * PIXEL_PER_LEVEL_UNIT) + 2;
        borderSR.size = new(maxWidth, borderSR.size.y);
        backgroundSR.size = new(maxWidth, backgroundSR.size.y);
        levelSR.size = new(levelWidth, levelSR.size.y);

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

        Debug.Log($"Level %: {levelPercent}");

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

    private void OnValidate()
    {
        if (level > max) level = max;

        UpdateIcons();
    }
}
