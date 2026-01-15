using System.Collections.Generic;
using UnityEngine;

public static class CollectionsExtensions
{
    public static T SelectRandom<T>(IReadOnlyList<T> list) => list[Random.Range(0, list.Count)];
}
