using UnityEngine;

[CreateAssetMenu(fileName = "MeleeWeaponSO", menuName = "Scriptable Objects/MeleeWeaponSO")]
public class MeleeWeaponSO : ScriptableObject
{
    [SerializeField] private Sprite menuIcon;
    [SerializeField] private GameObject weaponTemplate;

    public Sprite MenuIcon => menuIcon;
    public GameObject WeaponTemplate => weaponTemplate;
}
