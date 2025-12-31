using System.Collections.Generic;
using UnityEngine;

public class CollisionData : MonoBehaviour
{
    public enum Type
    {
        Damage = 0,
    }

    public enum Receiver
    {
        Player = 0,
        Enemy = 1,
    }

    public int strength;
    public Type type;
    public List<Receiver> receivers;
}
