using System;
using UnityEngine;

public class GameNode
{
    public Chunk chunk;
    public int x, y, z; //  Basically to represent the local position of the node in the chunk
    public int worldX, worldY, worldZ;
    public int tileID;

    public bool isWalkable;
    public bool hasNode;

    #region A * Pathfinding Data
    public int gCost;
    public int hCost;
    public int fCost;
    public GameNode cameFromNode;

    public int HeapIndex { get; set; }
    #endregion

    public GameNode(Chunk chunk, int x, int y, int z, int tileID, bool isWalkable, bool hasCube)
    {
        this.chunk = chunk;
        this.x = x;
        this.y = y;
        this.z = z;
        this.tileID = tileID;
        this.isWalkable = isWalkable;
        this.hasNode = hasCube;

        worldX = x + chunk.startPoint.x;
        worldY = y + chunk.startPoint.y;
        worldZ = z + chunk.startPoint.z;
    }

    //  Summary
    //      Logic to calculate the f cost of the node used in A * pathfinding algorithm
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public Vector3 GetWorldVectorFromPath()
    {
        return new Vector3(worldX, worldY, worldZ);
    }
}
