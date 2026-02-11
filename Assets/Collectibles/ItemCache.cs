using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemCache : MonoBehaviour
{
    [SerializeField] ScriptableObject collectibleSO;
    [SerializeField] Sprite collectedSprite;

    [Space]
    [Header("Debug")]
    [SerializeField] private bool isCollected = false;

    public bool IsCollected => isCollected;

    private ICollectible collectible;
    private SpriteRenderer sr;

    private void Awake()
    {
        collectible = collectibleSO as ICollectible;
        sr = GetComponent<SpriteRenderer>();

        if (collectible == null)
        {
            Debug.LogError($"{name} ({GetType().Name}): collectibleSO {collectibleSO.name} is not an ICollectible!");
        }
    }

    public void Collect(GameObject collector)
    {
        if (isCollected) return;
        isCollected = true;
        sr.sprite = collectedSprite;
        collectible.Collect(collector);
    }

    public interface ICollectible
    {
        public void Collect(GameObject collector);
    }
}
