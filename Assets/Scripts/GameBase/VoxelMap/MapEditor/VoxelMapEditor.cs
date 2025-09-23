using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor.Formats.Fbx.Exporter;
using TMPro;

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
public class Heatmap
{
    public const int HEATMAP_MAX_VALUE = 45;
    public const int HEATMAP_MIN_VALUE = 0;
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
    private string fbxExportPath = "MapModel.fbx";
    private string loadMapDataPath = "VoxelMap.json";
    private GameObject heatMapObject;
    private List<TextMeshPro> textMeshPros = new List<TextMeshPro>();
    private CharacterBase character;
    private Material heatMaterial;

    private Vector2 scrollPos;

    [MenuItem("Utils/VoxelMapEditor")]
    public static void ShowWindow()
    {
        VoxelMapEditor window = (VoxelMapEditor)GetWindow(typeof(VoxelMapEditor));
        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

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
        if (GUILayout.Button("Export FBX Combined Mesh"))
        {
            if (Selection.activeGameObject != null) { ExportGameObjectToFbx(Selection.activeGameObject); }
        }
        loadMapDataPath = EditorGUILayout.TextField("Map Data File Path", "VoxelMap.json");
        heatMaterial = (Material)EditorGUILayout.ObjectField("Heat Map Material", heatMaterial, typeof(Material), false);
        character = (CharacterBase)EditorGUILayout.ObjectField("Character", character, typeof(CharacterBase), true);
        if (GUILayout.Button("Show Movable Cost Map"))
        {
            SaveAndLoad.LoadMap(loadMapDataPath, out World world);
            GenerateVisibleCostMap(character, world);
        }
        if (GUILayout.Button("Remove Movable Cost Map"))
        {
            RemoveVisibleMapCost();
        }
        EditorGUILayout.EndScrollView();
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

    public GameObject GetCombineBlock(GameObject gridObject)
    {
        GetAllBlock(gridObject, out List<GameObject> blocks);
        BlockCombiner blockCombiner = new BlockCombiner(RedrawVisibleFace(blocks));
        return blockCombiner.GetCombinedMesh();
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
        string filePathFull = Path.Combine(Application.persistentDataPath, filePath);

        try
        {
            File.WriteAllText(filePathFull, jsonData);
            Debug.Log($"Successfully saved data to {filePathFull}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to save data to {filePathFull}. \n {exception}");
        }
    }

    private void ExportGameObjectToFbx(GameObject gridObject)
    {
        string fbxPathFull = Path.Combine(Application.dataPath, fbxExportPath);
        ExportModelOptions exportSettings = new ExportModelOptions();
        exportSettings.ExportFormat = ExportFormat.Binary;
        exportSettings.KeepInstances = false;
        GameObject mesh = GetCombineBlock(gridObject);
        ModelExporter.ExportObject(fbxPathFull, mesh, exportSettings);
        DestroyImmediate(mesh);
    }

    //  Summary
    //      Generate a heat map to show the cost of movement from the character's position
    //      The cost is calculated using Dijkstra's algorithm
    //      Only take the nodes which is walkable
    private void GenerateVisibleCostMap(CharacterBase character, World world)
    {
        PathFinding pathFinding = new PathFinding(world);

        DestroyImmediate(heatMapObject);
        textMeshPros.Clear();

        heatMapObject = new GameObject("HeatMapVisible");
        heatMapObject.transform.position = new Vector3(0, 0.55f, 0);
        MeshFilter meshFilter = heatMapObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = heatMapObject.AddComponent<MeshRenderer>();
        meshRenderer.material = heatMaterial;
        Mesh mesh = new Mesh();
        mesh.name = "HeatMapMesh";
        meshFilter.mesh = mesh;

        List<GameNode> dijsktraCostNode = pathFinding.CalculateDijkstraCostFromPos(character.transform.position, 1, 1);
        if (dijsktraCostNode == null || dijsktraCostNode.Count == 0)
        {
            Debug.LogWarning("No nodes returned from pathfinding.");
            return;
        }

        Utils.CreateEmptyMeshArrays(dijsktraCostNode.Count, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles);
        for (int i = 0; i < dijsktraCostNode.Count; i++)
        {
            int cost = dijsktraCostNode[i].dijkstraCost;

            float heatMapNornalizeValue = (float)cost / Heatmap.HEATMAP_MAX_VALUE;
            Vector3 position = dijsktraCostNode[i].GetVector();
            Vector2 heatMapUV = new Vector2(heatMapNornalizeValue, 0);
            Utils.AddToMeshArrays(vertices, uvs, triangles, i, position, 0f, new Vector3(1, 0, 1), heatMapUV, heatMapUV);

            string text = cost.ToString();
            textMeshPros.Add(Utils.CreateWorldText(text, position + Vector3.zero, Quaternion.Euler(90, 0, 0), 5, Color.white, TextAlignmentOptions.Center));
        }
        GameObject textParent = GameObject.Find("World_Text_Parent");
        if (textParent != null) { textParent.transform.position = new Vector3(0, 0.6f, 0); }
        textParent.transform.SetParent(heatMapObject.transform, true);
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    private void RemoveVisibleMapCost()
    {
        DestroyImmediate(heatMapObject);
        textMeshPros.Clear();
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