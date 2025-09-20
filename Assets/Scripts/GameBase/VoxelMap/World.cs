using System.Collections.Generic;
using UnityEngine;

public struct GridPosition
{
    public int x;
    public int z;
    public int height;

    public GridPosition(int x, int z, int height)
    {
        this.x = x;
        this.z = z;
        this.height = height;
    }
}

public class World
{
    //  Summary
    //      To store the loaded nodes in the world, which nodes could not be the local position of the chunk
    public Dictionary<Vector3Int, GameNode> loadedNodes = new Dictionary<Vector3Int, GameNode>();

    public int worldMaxX, worldMaxZ;
    public int worldMinX, worldMinZ;
    public int worldHeight;
    private float cellSize = 1f;

    public GameObject combinedMesh;

    public void InitializeMapNode(List<GameNodeData> nodeDataList)
    {
        for (int i = 0; i < nodeDataList.Count; i++)
        {
            int x = nodeDataList[i].x;
            int y = nodeDataList[i].y;
            int z = nodeDataList[i].z;
            bool isWalkable = nodeDataList[i].isWalkable;
            bool hasNode = nodeDataList[i].hasNode;
            if (!loadedNodes.ContainsKey(new Vector3Int(x, y, z)))
            {
                GameNode gameNode = new GameNode(x, y, z, isWalkable, hasNode);
                loadedNodes.Add(new Vector3Int(x, y, z), gameNode);
                UpdateWorldSize(x, y, z);
            }
        }
    }

    public void GenerateNode(int x, int height, int z)
    {
        if (AddNode(x, height, z))
        {
            AdjustCoverNode(x, height, z);
        }
    }

    private bool AddNode(int x, int y, int z)
    {
        if (!loadedNodes.ContainsKey(new Vector3Int(x, y, z)))
        {
            GameNode gameNode = new GameNode(x, y, z, true, true);
            loadedNodes.Add(new Vector3Int(x, y, z), gameNode);
            UpdateWorldSize(x, y, z);
            return true;
        }
        return false;
    }

    private void AdjustCoverNode(int x, int y, int z)
    {
        GameNode node = GetNode(x, y, z);
        GameNode aboveNode = GetNode(x, y + 1, z);
        if (aboveNode != null && aboveNode != null && aboveNode.hasNode)
        {
            node.isWalkable = false;
        }
    }

    private void UpdateWorldSize(int x, int height, int z)
    {
        worldMaxX = Mathf.Max(worldMaxX, x);
        worldMaxZ = Mathf.Max(worldMaxZ, z);

        worldMinX = Mathf.Min(worldMinX, x);
        worldMinZ = Mathf.Min(worldMinZ, z);

        worldHeight = Mathf.Max(worldHeight, height);
        //Debug.Log($"World size: {worldMaxX} {worldMaxZ}");
    }

    public void GetWorldPosition(Vector3 worldPosition, out int x, out int y, out int z)
    {
        x = Mathf.FloorToInt(worldPosition.x);
        y = Mathf.FloorToInt(worldPosition.y);
        z = Mathf.FloorToInt(worldPosition.z);
    }

    public GameNode GetNode(Vector3 position)
    {
        GameNode node;
        if (loadedNodes.TryGetValue(new Vector3Int((int)position.x, (int)position.y, (int)position.z), out node))
            return node;
        return null;
    }
    
    public GameNode GetNode(int x, int y, int z)
    {
        GameNode node;
        if (loadedNodes.TryGetValue(new Vector3Int(x, y, z), out node))
            return node;
        return null;
    }

    public GameNode GetHeightNodeWithCube(int x, int z)
    {
        if (x > worldMaxX || x < worldMinX || z > worldMaxZ || z < worldMinZ) { return null; }

        for (int y = worldHeight; y >= 0; y--)
        {
            if (loadedNodes.TryGetValue(new Vector3Int(x, y, z), out GameNode node))
            {
                if (node.hasNode) return node;
            }   
        }
        return null;
    }

    public Vector3 GetCellOffsetPosition(Vector3 position)
    {
        float halfCell = cellSize / 2f;
        float y = position.y - halfCell;
        return new Vector3(position.x, y, position.z);
    }

    #region External
    //  Summary
    //      To check if the input position is valid in the world.
    public bool IsValidWorldRange(Vector3 position)
    {
        Vector3 localPosition = GetCellOffsetPosition(position);
        if (worldMinX <= localPosition.x && 0 <= localPosition.y && worldMinZ <= localPosition.z
        && worldMaxX >= localPosition.x && worldHeight >= localPosition.y && worldMaxZ >= localPosition.z)
        {
            Debug.Log($"{localPosition} is valid");
            return true;
        }
        Debug.Log($"{localPosition} is invalid");
        return false;
    }

    public bool IsValidNode(float x, float y, float z)
    {
        return worldMinX <= x && 0 <= y && worldMinZ <= z
            && worldMaxX >= x && worldHeight >= y && worldMaxZ >= z;
    }
    #endregion

    #region Manhattan Distance Logic
    //  Summary
    //      This function calculates the Manhattan distance range in 3D space
    public List<Vector3Int> GetManhattas3DRange(
        Vector3Int unitPosition,
        int size,
        bool checkWalkable = true,
        bool limitY = false,
        int yLength = 0)
    {
        List<Vector3Int> coverage = new List<Vector3Int>();

        if (limitY && size > yLength) { size = yLength; }

        int minX = unitPosition.x - size;
        int maxX = unitPosition.x + size;
        int minY = unitPosition.y - size;
        int maxY = unitPosition.y + size;
        int minZ = unitPosition.z - size;
        int maxZ = unitPosition.z + size;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    // Check if the current position is within the Manhattan distance range
                    int manhattasDistance = Mathf.Abs(unitPosition.x - x)
                             + Mathf.Abs(unitPosition.y - y)
                             + Mathf.Abs(unitPosition.z - z);
                    if (manhattasDistance > size) continue;
                    if (!IsValidNode(x, y, z)) continue;

                    GameNode node = GetNode(x, y, z);
                    if (node == null) continue;

                    if (checkWalkable && !node.isWalkable)
                        continue;

                    coverage.Add(new Vector3Int(x, y, z));
                }
            }
        }
        return coverage;
    }
    #endregion
}