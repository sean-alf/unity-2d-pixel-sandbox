using UnityEngine;

public class CollisionData : MonoBehaviour
{
    public enum CollisionType
    {
        Damage = 0,
    }

    [SerializeField]
    private int strength;

    [SerializeField]
    private CollisionType type;

    [SerializeField]
    private string id;

    public int Strength => strength;
    public CollisionType Type => type;
    public string ID => id;
}
