using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [Serializable]
    public class BlockData
    {
        public int prefabID;
        public int x, y, z;
    }

    [Serializable]
    public class ChunkData
    {
        public List<BlockData> blockDataList;
    }

    public ChunkData chunkData;
    [SerializeField] private PrefabDatabaseSO prefabDatabase;
    [SerializeField] private string filePath;

    public void SerializeBlockData(Dictionary<(int, int, int), Chunk> chunks)
    {
        chunkData = new ChunkData();
        chunkData.blockDataList = new List<BlockData>();

        foreach (var kvp in chunks)
        {
            (int, int, int) chunkPosition = kvp.Key;
            Chunk chunk = kvp.Value;

            foreach (var kvpChunk in chunk.blocks)
            {
                Vector3Int localPosition = kvpChunk.Key;
                GameObject prefab = kvpChunk.Value;

                if (prefabDatabase != null)
                {
                    Debug.Log($"Prefab: {prefab.name}");
                    int prefabID = prefabDatabase.GetPrefabID(prefab.name);
                    if (prefabID == -1)
                    {
                        Debug.Log("Prefab not found in PrefabDatabaseSO");
                        continue;
                    }
                    chunkData.blockDataList.Add(new BlockData { prefabID = prefabID, x = localPosition.x, 
                        y = localPosition.y, z = localPosition.z });
                }
                else { Debug.Log("PrefabDatabaseSO is null, need to be assigned"); }
            }
        }
        Save();
    }
    public void Save()
    {
        filePath = Application.persistentDataPath + "/save.data";

        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            writer.Write(chunkData.blockDataList.Count); // 先写入数据数量
            foreach (var block in chunkData.blockDataList)
            {
                writer.Write(block.prefabID);
                writer.Write(block.x);
                writer.Write(block.y);
                writer.Write(block.z);
            }
        }
        Debug.Log($"Save filepath at {filePath}");
        byte[] data = File.ReadAllBytes(Application.persistentDataPath + "/save.data");
        Debug.Log(BitConverter.ToString(data));
    }


}