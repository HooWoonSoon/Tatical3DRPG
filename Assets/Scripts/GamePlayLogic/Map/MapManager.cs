using UnityEngine;
using System;

[Serializable]
public class PresetUnit
{
    public CharacterBase character;
    public Vector3Int deployPos;
}

[Serializable]
public class MapData
{
    public string mapDataPath;
    public GameObject mapModel;
    public bool requireDeployment;
    public PresetUnit[] presetUnits;
}
public class MapManager : MonoBehaviour
{
    public World world;
    public MapData[] mapDatas;
    public MapData currentActivatedMap { get; private set; }
    public static MapManager instance { get; private set;}

    private void Awake()
    {
        instance = this;
        InitializeMap(mapDatas[0]);
    }

    public void InitializeMap(MapData mapData)
    {
        InitializeMap(mapData.mapDataPath, mapData.mapModel);
        currentActivatedMap = mapData;
    }
    private void InitializeMap(string mapDataPath, GameObject mapModel)
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

    public void SwitchMap(MapData mapData)
    {
        SwitchMap(mapData.mapDataPath, mapData.mapModel);
        currentActivatedMap = mapData;
    }

    private void SwitchMap(string mapDataPath, GameObject mapModel)
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