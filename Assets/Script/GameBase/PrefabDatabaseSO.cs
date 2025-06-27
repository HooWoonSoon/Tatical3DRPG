using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "Blocks/Block")]
public class PrefabDatabaseSO : ScriptableObject
{
    public List<PrefabEntry> prefabs = new List<PrefabEntry>();
    private Dictionary<int, GameObject> prefabPairs = new Dictionary<int, GameObject>();

    [Serializable]
    public class PrefabEntry
    {
        public GameObject prefab;
        public int prefabID;
    }

    private void Initialize()
    {
        if (prefabPairs.Count > 0) return;

        foreach (var prefab in prefabs)
        {
            if (!prefabPairs.ContainsKey(prefab.prefabID))
                prefabPairs.Add(prefab.prefabID, prefab.prefab);
                Debug.Log(prefab.prefabID + " " + prefab.prefab.name);
        }
        Debug.Log("Prefab Database Initialized");
    }

    public GameObject GetPrefab(int prefabID)
    {
        Initialize();
        if (prefabPairs.TryGetValue(prefabID, out GameObject prefab)) { return prefab; }
        else { return null; }
    }

    public int GetPrefabID(string prefabname)
    {
        Initialize();
        foreach (var prefabPair in prefabPairs)
        {
            if (prefabPair.Value.name == prefabname)
            {
                Debug.Log($"Prefab ID: {prefabPair.Key}");
                return prefabPair.Key;
            }
        }
        return -1;
    }
}