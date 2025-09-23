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
        SaveAndLoad.LoadMap(mapDataPath, out world);
    }

    private void Start()
    {
        //GenerateBlock();
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