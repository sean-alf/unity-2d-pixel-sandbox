using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSO", menuName = "Scriptable Objects/ProjectileSO")]
public class ProjectileSO : ScriptableObject, ItemCache.ICollectible
{
    [Header("Menu/Dialog Sprite")]
    [SerializeField] private Sprite menuIcon;
    [Header("Item Cache Sprite")]
    [SerializeField] private Sprite cacheIcon;
    [SerializeField] private string displayName;
    [SerializeField][TextArea] private string description = "No description yet";
    [SerializeField] private Sprite playerEye;
    [SerializeField] private GameObject template;

    public Sprite LargeIcon => menuIcon;
    public Sprite SmallIcon => cacheIcon;
    public string DisplayName => displayName != null && displayName.Length > 0 ? displayName : template.name;
    public string Description => description;
    public Sprite PlayerEye => playerEye;

    public void Collect(GameObject collector)
    {
        var projectileManager = collector.GetComponent<ProjectileManager>();
        projectileManager.AddProjectile(this);
    }

    public void Instantiate(Action<BasicProjectile> onNotNull)
    {
        if (template == null)
        {
            Debug.LogError($"ProjectileManager ({name}): template is null!!");
            return;
        }

        if (Instantiate(template, Vector3.zero, Quaternion.identity).TryGetComponent(out BasicProjectile p))
        {
            onNotNull(p);
        }
        else
        {
            Debug.LogError($"ProjectileManager ({name}: invalid projectileTemplate, no BasicProjectile attached!!!");
        }
    }
}
