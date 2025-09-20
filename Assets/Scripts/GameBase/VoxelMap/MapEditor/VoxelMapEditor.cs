using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;

[Serializable]
public class GameNodeData
{
    public int x, y, z;
    public bool isWalkable;
    public bool hasNode;

    public GameNodeData(int x, int y, int z, bool isWalkable, bool hasNode)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.isWalkable = isWalkable;
        this.hasNode = hasNode;
    }
}

#region Structor
public struct BlockFace
{
    public Vector3 position;
    public List<Vector3Int> normal;
    public Material material;
}
#endregion
public class VoxelMapEditor : EditorWindow
{
    private float gridOffsetXZ = 0.5f;

    private bool hideBlockMergeToggle = true;

    private string filePath = "VoxelMap.json";

    [MenuItem("Utils/VoxelMapEditor")]
    public static void ShowWindow()
    {
        VoxelMapEditor window = (VoxelMapEditor)GetWindow(typeof(VoxelMapEditor));
        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Grid Map Properties", EditorStyles.boldLabel);
        gridOffsetXZ = EditorGUILayout.FloatField("Grid Offset X and Z", gridOffsetXZ);

        if (GUILayout.Button("Add new Level"))
        {
            if (Selection.activeGameObject != null) { AddNewLevel(Selection.activeGameObject); }
        }
        hideBlockMergeToggle = EditorGUILayout.ToggleLeft("Hide blocks after merging mesh", hideBlockMergeToggle);
        if (GUILayout.Button("Redefine block properties with covered block"))
        {
            if (Selection.activeGameObject != null) { RedefineBlockProperities(Selection.activeGameObject); }
        }
        if (GUILayout.Button("Combine Block"))
        {
            if (Selection.activeGameObject != null) { CombineBlock(Selection.activeGameObject); }
        }
        if (GUILayout.Button("Reactive hided blocks")) 
        {
            if (Selection.activeGameObject != null) { ReactiveHidedBlocks(Selection.activeGameObject); }
        }
        filePath = EditorGUILayout.TextField("Save JSON File Path", filePath);
        if (GUILayout.Button("Save JSON Map"))
        {
            if (Selection.activeGameObject != null) { SaveJSONData(Selection.activeGameObject); }
        }
    }

    #region Function For Editor
    private void AddNewLevel(GameObject gridObject)
    {
        TilemapList3D tilemapList3D = gridObject.GetComponent<TilemapList3D>();
        Grid grid = gridObject.GetComponent<Grid>();
        if (tilemapList3D != null)
        {
            if (tilemapList3D.LayerCount() > 16)
            {
                Debug.Log("The number of layer exceeds 16, " +
                    "you can't out to the maximum value of the world height");
                return;
            }

            tilemapList3D.ResetTileMapList(gridOffsetXZ);
            GameObject newLayer = new GameObject();
            newLayer.AddComponent<Tilemap>();
            newLayer.AddComponent<TilemapRenderer>();

            newLayer.name = "Level (" + tilemapList3D.LayerCount() + ")";

            newLayer.transform.parent = grid.transform;
            float newHeightLevel = tilemapList3D.LayerCount() * grid.cellSize.y * grid.transform.localScale.y;
            newLayer.transform.position = new Vector3(gridOffsetXZ, newHeightLevel, gridOffsetXZ);
            newLayer.transform.localScale = Vector3.one;

            tilemapList3D.AddLayer(newLayer, gridOffsetXZ);
            Debug.Log($"Add new Level: {newLayer.name}");
        }
    }

    private void RedefineBlockProperities(GameObject gridObject)
    {
        GetAllBlock(gridObject, out List<GameObject> blocks);
        HashSet<Vector3Int> blockPositions = new HashSet<Vector3Int>();
        foreach (var block in blocks)
        {
            blockPositions.Add(Vector3Int.RoundToInt(block.transform.position));
        }
        Debug.Log(blocks.Count);

        for (int i = 0; i < blocks.Count; i++)
        {
            Vector3Int position = Vector3Int.RoundToInt(blocks[i].transform.position);
            BlockProperites properities = blocks[i].GetComponent<BlockProperites>();

            Vector3Int abovePosition = position + Vector3Int.up;
            if (blockPositions.Contains(abovePosition))
            {
                properities.isWalkable = false;
            }
        }
    }

    public void CombineBlock(GameObject gridObject)
    {
        GetAllBlock(gridObject, out List<GameObject> blocks);
        BlockCombiner blockCombiner = new BlockCombiner(RedrawVisibleFace(blocks));
    }

    private void ReactiveHidedBlocks(GameObject gridObject)
    {
        Grid grid = gridObject.GetComponent<Grid>();
        List<Transform> allLayer = new List<Transform>();
        foreach (Transform layer in grid.transform) { allLayer.Add(layer); }
        foreach (Transform layer in allLayer)
        {
            foreach(Transform child in layer)
            {
                if (!child.gameObject.activeSelf) { child.gameObject.SetActive(true); }
            }
        }
    }

    private void SaveJSONData(GameObject gridObject)
    {
        GetAllBlock(gridObject, out List<GameObject> blocks);

        List<GameNodeData> gameNodesData = new List<GameNodeData>();
        for (int i = 0; i < blocks.Count; i++)
        {
            BlockProperites properites = blocks[i].GetComponent<BlockProperites>();
            GameNodeData gameNodeData = new GameNodeData(
                Mathf.RoundToInt(blocks[i].transform.position.x),
                Mathf.RoundToInt(blocks[i].transform.position.y),
                Mathf.RoundToInt(blocks[i].transform.position.z),
                properites.isWalkable,
                properites.hasCube
                );
            gameNodesData.Add(gameNodeData);
        }

        string jsonData = JsonConvert.SerializeObject(gameNodesData, Formatting.Indented);
        string filePathFull = Path.Combine(Application.dataPath, filePath);

        try
        {
            File.WriteAllText(filePathFull, jsonData);
            Debug.Log($"Successfully saved data to {filePathFull}");
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Failed to save data to {filePathFull}. \n {exception}");
        }
    }
    #endregion

    private void GetAllBlock(GameObject gridObject, out List<GameObject> blocks)
    {
        Grid grid = gridObject.GetComponent<Grid>();
        List<Transform> allLayer = new List<Transform>();

        blocks = new List<GameObject>();

        foreach (Transform layer in grid.transform)
        {
            allLayer.Add(layer);
        }

        for (int i = 0; i < allLayer.Count; i++)
        {
            Transform parentTransform = allLayer[i];
            foreach (Transform child in parentTransform)
            {
                if (child.GetComponent<BlockProperites>() != null)
                {
                    blocks.Add(child.gameObject);
                    if (hideBlockMergeToggle) { child.gameObject.SetActive(false); }
                }
            }
        }
    }

    #region Combine Block
    private Dictionary<Vector3, BlockFace> RedrawVisibleFace(List<GameObject> blocks)
    {
        Dictionary<Vector3, BlockFace> visibleFaces = new Dictionary<Vector3, BlockFace>();
        HashSet<Vector3> blocksPosition = new HashSet<Vector3>();
        foreach (var block in blocks)
        {
            blocksPosition.Add(Vector3Int.RoundToInt(block.transform.position));
        }

        for (int i = 0; i < blocks.Count; i++)
        {
            Vector3 blockPosition = blocks[i].transform.position;

            List<Vector3Int> visibleNormal = new List<Vector3Int>();
            Vector3Int[] directions =
            {
                Vector3Int.right, Vector3Int.left, Vector3Int.forward,
                Vector3Int.back, Vector3Int.up, Vector3Int.down
            };

            foreach (Vector3Int direction in directions)
            {
                Vector3 neighbourPos = blocks[i].transform.position + direction;
                if (!blocksPosition.Contains(neighbourPos))
                {
                    visibleNormal.Add(direction);
                }
            }

            Material blockMaterial = null;
            MeshRenderer renderer = blocks[i].GetComponent<MeshRenderer>();
            if (renderer != null) { blockMaterial = renderer.sharedMaterial; }

            if (visibleNormal.Count > 0)
            {
                visibleFaces[blockPosition] = new BlockFace
                {
                    position = blockPosition,
                    normal = visibleNormal,
                    material = blockMaterial
                };
            }
        }
        return visibleFaces;
    }
    #endregion
}