using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSO", menuName = "Scriptable Objects/ProjectileSO")]
public class ProjectileSO : ScriptableObject
{
    [SerializeField]
    private Sprite menuIcon;

    [SerializeField]
    private GameObject template;

    [SerializeField]
    private int maxProjectiles = 5;

    public Sprite MenuIcon => menuIcon;
    public int MaxProjectiles => maxProjectiles;

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
