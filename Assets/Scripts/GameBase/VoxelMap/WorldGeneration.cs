using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public class WorldGeneration : MonoBehaviour
{
    public World world;
    public GameObject prefab;
    public string mapDataPath;
    public static WorldGeneration instance { get; private set;}

    private void Awake()
    {
        instance = this;
        world = new World();
    }

    private void Start()
    {
        LoadMap();
        //GenerateBlock();
    }

    public void LoadMap()
    {
        string fullPath = Path.Combine(Application.dataPath, mapDataPath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Map data file not found at path: {fullPath}");
            return;
        }
        string json = File.ReadAllText(fullPath);
        List<GameNodeData> nodeDataList = JsonConvert.DeserializeObject<List<GameNodeData>>(json);
        world.InitializeMapNode(nodeDataList);
    }

    //Debug
    public void GenerateBlock()
    {
        if (world == null) return;

        for (int x = 0; x < 33; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 33; z++)
                {
                    world.GenerateNode(x, y, z);
                    GameNode node = world.GetNode(x, y, z);
                    if (node.hasCube)
                    {
                        Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                    }
                }
            }
        }

        world.GenerateNode(3, 4, 3);
        GameNode node1 = world.GetNode(3, 4, 3);
        if (node1.hasCube)
        {
            Instantiate(prefab, new Vector3(3, 4, 3), Quaternion.identity);
        }
    }
}