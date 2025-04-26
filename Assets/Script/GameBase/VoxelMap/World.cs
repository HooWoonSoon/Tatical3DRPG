using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;

public class World
{
    //  Summary
    //      To store the loaded nodes in the world, which nodes could not be the local position of the chunk
    public Dictionary<(int, int, int), GameNode> loadedNodes = new Dictionary<(int, int, int), GameNode>();

    public Dictionary<(int, int, int), Region> regions;
    public const int REGION_SIZE = 32;

    public int worldMaxX, worldMaxY, worldMaxZ;
    public int worldMinX, worldMinY, worldMinZ;

    public World()
    {
        regions = new Dictionary<(int, int, int), Region>();
    }

    public void UpdateWorldSize()
    {
        foreach (var region in regions.Values)
        {
            foreach (var chunk in region.loadedChunks.Values)
            {
                if (chunk.endPoint.x > worldMaxX) worldMaxX = chunk.endPoint.x;
                if (chunk.endPoint.y > worldMaxY) worldMaxY = chunk.endPoint.y;
                if (chunk.endPoint.z > worldMaxZ) worldMaxZ = chunk.endPoint.z;

                if (chunk.startPoint.x < worldMinX) worldMinX = chunk.startPoint.x;
                if (chunk.startPoint.y < worldMinY) worldMinY = chunk.startPoint.y;
                if (chunk.startPoint.z < worldMinZ) worldMinZ = chunk.startPoint.z;
            }
        }
        //Debug.Log($"World size: {worldMaxX} {worldMaxY} {worldMaxZ}");
    }

    #region Generate Chunk And Node
    public Chunk GetChunk(int chunkX, int chunkY, int chunkZ)
    {
        int regionX = Mathf.FloorToInt(chunkX / REGION_SIZE);
        int regionY = 0;
        int regionZ = Mathf.FloorToInt(chunkZ / REGION_SIZE);

        if (!regions.ContainsKey((regionX, regionY, regionZ)))
        {
            regions[(regionX, regionY, regionZ)] = new Region(regionX, regionY, regionZ);
        }

        Chunk chunk = regions[(regionX, regionY, regionZ)].GetChunk(chunkX, chunkY, chunkZ);
        foreach (var node in chunk.nodes)
        {
            loadedNodes[(node.worldX, node.worldY, node.worldZ)] = node;
        }
        UpdateWorldSize();
        return chunk;
    }
    #endregion

    #region Get world position
    public void GetWorldPosition(Vector3 worldPosition, out int x, out int y, out int z)
    {
        x = Mathf.FloorToInt(worldPosition.x);
        y = Mathf.FloorToInt(worldPosition.y);
        z = Mathf.FloorToInt(worldPosition.z);
    }
    #endregion

    #region Get Chunk Position
    public void GetChunkPosition(int x, int y, int z, out int chunkX, out int chunkY, out int chunkZ)
    {
        chunkX = Mathf.FloorToInt(x / Chunk.CHUNK_SIZE);
        chunkY = Mathf.FloorToInt(y / Chunk.CHUNK_SIZE);
        chunkZ = Mathf.FloorToInt(z / Chunk.CHUNK_SIZE);
    }

    public void GetChunkPosition(Vector3 worldPosition, out int chunkX, out int chunkY, out int chunkZ)
    {
        chunkX = Mathf.FloorToInt(worldPosition.x / Chunk.CHUNK_SIZE);
        chunkY = Mathf.FloorToInt(worldPosition.y / Chunk.CHUNK_SIZE);
        chunkZ = Mathf.FloorToInt(worldPosition.z / Chunk.CHUNK_SIZE);
    }
    #endregion

    #region Get Node At World Position

    //  Summary
    //      To get the node at the world position, it used for A * pathfinding algorithm,
    //      because world have been seperate into the chunk, when the target position is overstep the boundaries
    //      need to searching chunk again. This is a part of encapsulation.
    public GameNode GetNodeAtWorldPosition(int x, int y, int z)
    {
        GetChunkPosition(x, y, z, out int chunkX, out int chunkY, out int chunkZ);
        Chunk chunk = GetChunk(chunkX, chunkY, chunkZ);
        if (chunk == null) { Debug.Log("Could not get the chunk node"); return null; }

        int localX = (x % Chunk.CHUNK_SIZE + Chunk.CHUNK_SIZE) % Chunk.CHUNK_SIZE;
        int localY = (y % Chunk.CHUNK_SIZE + Chunk.CHUNK_SIZE) % Chunk.CHUNK_SIZE;
        int localZ = (z % Chunk.CHUNK_SIZE + Chunk.CHUNK_SIZE) % Chunk.CHUNK_SIZE;

        return chunk.GetNode(localX, localY, localZ);
    }
    #endregion

    public void ReleaseRegion(Region region)
    {
        foreach(var kvp in regions)
        {
            if (kvp.Value == region)
            {
                regions.Remove(kvp.Key);
                break;
            }
        }
    }

    #region external

    //  Summary
    //      To check if the input position is valid in the world.
    public bool IsValidNode(Vector3 position)
    {
        return worldMinX < position.x && worldMinY < position.y && worldMinZ < position.z
        && worldMaxX > position.x && worldMaxY > position.y && worldMaxZ > position.z;
    }

    public bool IsValidNode(float x, float y, float z)
    {
        return worldMinX < x && worldMinY < y && worldMinZ < z
            && worldMaxX > x && worldMaxY > y && worldMaxZ > z;
    }
    #endregion
}