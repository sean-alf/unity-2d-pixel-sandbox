using UnityEngine;

public class CollisionData : MonoBehaviour
{
    public enum Type
    {
        Damage = 0,
    }

    public int strength;
    public Type type;
}
