using UnityEngine;

using System;

[CreateAssetMenu(fileName = "PlayerProjectileSO", menuName = "Scriptable Objects/PlayerProjectileSO")]
public class PlayerProjectileSO : ScriptableObject, ItemCache.ICollectible
{
    [Header("Menu/Dialog Sprite")]
    [SerializeField] private Sprite menuIcon;
    [Header("Item Cache Sprite")]
    [SerializeField] private Sprite cacheIcon;
    [SerializeField] private string displayName;
    [SerializeField][TextArea] private string description = "No description yet";
    [SerializeField] private Sprite playerEye;
    [SerializeField] private ProjectileSO projectileSO;

    public ProjectileSO Projectile => projectileSO;
    public Sprite LargeIcon => menuIcon;
    public Sprite SmallIcon => cacheIcon;
    public string DisplayName => displayName != null && displayName.Length > 0 ? displayName : projectileSO.name;
    public string Description => description;
    public Sprite PlayerEye => playerEye;

    public void Collect(GameObject collector)
    {
        var projectileStash = collector.GetComponent<PlayerProjectileStash>();
        projectileStash.AddProjectile(this);
    }
}
