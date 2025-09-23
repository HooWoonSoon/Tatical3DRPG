using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public static class SaveAndLoad
{
    public static void LoadMap(string mapDataPath, out World world)
    {
        world = new World();
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
}

