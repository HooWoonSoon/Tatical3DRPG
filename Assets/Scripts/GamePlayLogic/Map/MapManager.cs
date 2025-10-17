using UnityEngine;
using System;

[Serializable]
public class MapData
{
    public string mapDataPath;
    public GameObject mapModel;
    public bool requireDeployment;
}
public class MapManager : MonoBehaviour
{
    public World world;
    public MapData[] mapDatas;
    public static MapManager instance { get; private set;}

    private void Awake()
    {
        instance = this;
        InitializeMap(mapDatas[0].mapDataPath, mapDatas[0].mapModel);
    }

    public void InitializeMap(string mapDataPath, GameObject mapModel)
    {
        SaveAndLoad.LoadMap(mapDataPath, out world);

        foreach (MapData mapData in mapDatas)
        {
            if (mapData != null)
                mapData.mapModel.SetActive(false);
        }

        if (mapModel != null)
            mapModel.SetActive(true);
    }

    public void SwitchMap(string mapDataPath, GameObject mapModel)
    {
        GridTilemapVisual.instance.DeSubcribeAllNodes();
        SaveAndLoad.LoadMap(world, mapDataPath, () =>
        {
            GridTilemapVisual.instance.SubscribeAllNodes();
            GridCharacter.instance.SubscribeAllNodes();
        });

        foreach (MapData mapData in mapDatas)
        {
            if (mapData != null)
                mapData.mapModel.SetActive(false);
        }

        if (mapModel != null)
            mapModel.SetActive(true);
    }
}