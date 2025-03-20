using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class ChunkList3D : MonoBehaviour
{
    public List<GameObject> chunks;
    public GameObject chunkManager;
    public void AddChunk(GameObject chunk)
    {
        if (chunkManager == null)
        {
            chunkManager = new GameObject();
            chunkManager.name = "Chunk Manager";
        }

        chunks.RemoveAll(item => item == null);

        List<GameObject> toRemove = new List<GameObject>();
        foreach (var existChunk in chunks)
        {
            if (existChunk != null && existChunk.name == chunk.name)
            {
                toRemove.Add(existChunk);
            }
        }

        foreach (var removeChunk in toRemove)
        {
            chunks.Remove(removeChunk);
            DestroyImmediate(removeChunk);
        }
        chunk.transform.SetParent(chunkManager.transform);
        chunks.Add(chunk);
    }
}