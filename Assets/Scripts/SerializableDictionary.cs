using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [Serializable]
    public struct KVP
    {
        public TKey key;
        public TValue value;
    }

    [SerializeField] private List<KVP> kvpList = new();

    public void OnAfterDeserialize()
    {
        Clear();
        for (int i = 0; i < kvpList.Count; i++)
        {
            var kvp = kvpList[i];
            this[kvp.key] = kvp.value;
        }
    }

    public void OnBeforeSerialize()
    {
        // Not needed since [Serializable] already serializes the fields
    }
}
