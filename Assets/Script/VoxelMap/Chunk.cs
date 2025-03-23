using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    #region Chunk Data
    public Vector3Int startPoint, endPoint;
    public const int CHUNK_SIZE = 16;
    public int cellSize = 1;
    public GameNode[,,] nodes;
    #endregion

    public Dictionary<Vector3Int, GameObject> blocks;
    public GameObject combinedMesh;

    #region Construct Variable
    public Chunk(int chunkX, int chunkY, int chunkZ)
    {
        startPoint = new Vector3Int(chunkX * CHUNK_SIZE, chunkY * CHUNK_SIZE, chunkZ * CHUNK_SIZE);
        endPoint = startPoint + new Vector3Int(CHUNK_SIZE - 1, CHUNK_SIZE - 1, CHUNK_SIZE - 1);

        nodes = new GameNode[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        blocks = new Dictionary<Vector3Int, GameObject>();

        InitializeNode();
    }

    private void InitializeNode()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    nodes[x, y, z] = new GameNode(this, x, y, z, 0, false, false);
                }
            }
        }
    }
    #endregion

    public void SetupNode(int x, int y, int z, bool isWalkable, bool hasCube)
    {
        nodes[x, y, z].hasCube = hasCube;
        nodes[x, y, z].isWalkable = isWalkable;
    }

    #region Chunk Manager
    public void AddBlock(Vector3Int worldPosition, GameObject block)
    {
        if (!blocks.ContainsKey(worldPosition))
        {
            int minX, minY, minZ;
            int maxX, maxY, maxZ;

            minX = startPoint.x; minY = startPoint.y; minZ = startPoint.z;
            maxX = endPoint.x; maxY = endPoint.y; maxZ = endPoint.z;

            if (worldPosition.x >= minX && worldPosition.y >= minY && worldPosition.z >= minZ &&
                worldPosition.x <= maxX && worldPosition.y <= maxY && worldPosition.z <= maxZ)
            {
                blocks.Add(GetLocalPosition(worldPosition), block);
            }
        }
    }
    public GameNode GetNode(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE) return null;
        return nodes[x, y, z];
    }
    #endregion

    #region Chunk Position Calculation
    public Vector3Int GetLocalPosition(Vector3 worldPosition)
    {
        int localX = Mathf.FloorToInt(worldPosition.x) % CHUNK_SIZE;
        int localY = Mathf.FloorToInt(worldPosition.y) % CHUNK_SIZE;
        int localZ = Mathf.FloorToInt(worldPosition.z) % CHUNK_SIZE;
        return new Vector3Int(localX, localY, localZ);
    }

    public void GetCellXZY(Vector3 worldPosition, out int x, out int y, out int z)
    {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
        z = Mathf.FloorToInt(worldPosition.z / cellSize);

        //  Limit the value of x, y, z to CHUNK_SIZE - 1 but it may be changed to width, height, depth
        if (x >= CHUNK_SIZE) x = CHUNK_SIZE - 1;
        if (y >= CHUNK_SIZE) y = CHUNK_SIZE - 1;
        if (z >= CHUNK_SIZE) z = CHUNK_SIZE - 1;
    }
    #endregion

    #region Structor
    public struct BlockFace
    {
        public Vector3Int position;
        public List<Vector3Int> normal;
        public Material material;
    }
    #endregion

    #region Combine Rendering Block
    public GameObject CombineBlockChunk()
    {
        if (combinedMesh == null)
        {
            BlockCombiner blockCombiner = new BlockCombiner(RedrawVisibleFace());
            combinedMesh = blockCombiner.GetCombinedMesh();
        }
        return combinedMesh;
    }

    private Dictionary<Vector3Int, BlockFace> RedrawVisibleFace()
    {
        Dictionary<Vector3Int, BlockFace> visibleFaces = new Dictionary<Vector3Int, BlockFace>();
        foreach (var kvp in blocks)
        {
            Vector3Int localPosition = kvp.Key;
            GameObject blockObject = kvp.Value;

            List<Vector3Int> visibleNormal = new List<Vector3Int>();
            Vector3Int[] directions =
            {
                Vector3Int.right, Vector3Int.left, Vector3Int.forward,
                Vector3Int.back, Vector3Int.up, Vector3Int.down
            };

            foreach (Vector3Int direction in directions)
            {
                Vector3Int neighbourPos = localPosition + direction;
                if (!blocks.ContainsKey(neighbourPos))
                {
                    visibleNormal.Add(direction);
                }
            }
          
            Material blockMaterial = null;
            MeshRenderer renderer = blockObject.GetComponent<MeshRenderer>();
            if (renderer != null) { blockMaterial = renderer.sharedMaterial; }

            if (visibleNormal.Count > 0)
            {
                visibleFaces[localPosition] = new BlockFace
                {
                    position = localPosition,
                    normal = visibleNormal,
                    material = blockMaterial
                };
            }
        }
        return visibleFaces;
    }
    #endregion
}