using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public World world;
    private BlockCombiner blockCombiner;
    [SerializeField] private int numChunkX = 80;
    [SerializeField] private int numChunkY = 1;
    [SerializeField] private int numChunkZ = 80;

    [SerializeField] private int preloadedRange = 9;
    public GameObject prefab;
    public static WorldManager instance { get; private set;}

    private void Awake()
    {
        instance = this;
        world = new World();
    }

    private void Start()
    {
        world.GetChunkPosition(transform.position, out int chunkX, out int chunkY, out int chunkZ);
        Chunk chunk = world.GenearateAndGetChunk(chunkX, chunkY, chunkZ);
        GenerateBlock();
    }

    //private void Update()
    //{
        //GetXZY(transform.position, out int x, out int y, out int z);
        //GenerateBlock();
    //}

    //public void GetXZY(Vector3 worldPosition, out int x, out int y, out int z)
    //{
    //    world.GetChunkPosition(worldPosition, out int chunkX, out int chunkY, out int chunkZ);

    //    Chunk chunk = world.GenearateAndGetChunk(chunkX, chunkY, chunkZ);

    //    chunk.GetCellXZY(worldPosition, out x, out y, out z);
    //}

    //  Debug
    public void GenerateBlock()
    {
        if (world == null) return;

        foreach (var regionPair in world.regions)
        {
            Region region = regionPair.Value;

            foreach (var chunkPair in region.loadedChunks)
            {
                Chunk chunk = chunkPair.Value;

                for (int cx = 0; cx < Chunk.CHUNK_SIZE; cx++)
                {
                    for (int cy = 0; cy < Chunk.CHUNK_SIZE - 12; cy++)
                    {
                        for (int cz = 0; cz < Chunk.CHUNK_SIZE; cz++)
                        {
                            GameNode node = chunk.GetNode(cx, cy, cz);
                            if (node != null)
                            {
                                Vector3Int pos = new Vector3Int(
                                    node.x + chunk.startPoint.x * Chunk.CHUNK_SIZE,
                                    node.y + chunk.startPoint.y * Chunk.CHUNK_SIZE,
                                    node.z + chunk.startPoint.z * Chunk.CHUNK_SIZE
                                );

                                if (pos == new Vector3(1,3,4) || pos == new Vector3(1,3,2) || pos == new Vector3(2,3,4)) { continue; }
                                if (!chunk.blocks.ContainsKey(pos))
                                {
                                    chunk.AddBlock(pos, prefab);
                                    chunk.SetupNode(cx, cy, cz, true, true);
                                }
                            }
                        }
                    }
                }

                for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < Chunk.CHUNK_SIZE - 12; y++)
                    {
                        for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                        {
                            GameNode node = chunk.GetNode(x, y, z);
                            GameNode aboveNode = chunk.GetNode(x, y + 1, z);
                            if (aboveNode.hasNode != false)
                            {
                                node.isWalkable = false;
                            }
                        }
                    }
                }

                if (chunk.combinedMesh == null)
                {
                    GameObject combinedMeshObject = chunk.CombineBlockChunk();
                    chunk.combinedMesh = combinedMeshObject;
                }
            }
        }
    }
}