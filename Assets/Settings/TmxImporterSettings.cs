using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TilemapSettings
{
    public string name;
    public int layer;
    public int sortingOrder;
}

[CreateAssetMenu(fileName = "TmxImporterSettings", menuName = "Settings/TmxImporterSettings")]
public class TmxImporterSettings : ScriptableObject
{
    public List<TilemapSettings> tilemapSettingsList;
}
