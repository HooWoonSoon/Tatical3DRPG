using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class VoxelMapEditor : EditorWindow
{
    private bool hideBlockMergeToggle = true;

    [MenuItem("Utils/VoxelMapEditor")]
    public static void ShowWindow()
    {
        VoxelMapEditor window = (VoxelMapEditor)GetWindow(typeof(VoxelMapEditor));
        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Add new Level"))
        {
            if (Selection.activeGameObject != null) { AddNewLevel(Selection.activeGameObject); }
        }
        if (GUILayout.Button("Merge chunk of the block")) 
        {
            if (Selection.activeGameObject != null) { CombineChunkOfBlock(Selection.activeGameObject); }
        }
        hideBlockMergeToggle = EditorGUILayout.ToggleLeft("Hide blocks after merging mesh", hideBlockMergeToggle);
        if (GUILayout.Button("Save Binary Data"))
        {
            if (Selection.activeGameObject != null) { SavaBinaryData(Selection.activeGameObject); }
        }
        if (GUILayout.Button("Reactive hided blocks")) 
        {
            if (Selection.activeGameObject != null) { ReactiveHidedBlocks(Selection.activeGameObject); }
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
                    "you can't out to the maximum value of the chunk height");
                return;
            }
            tilemapList3D.ResetTileMapList();
            GameObject newLayer = new GameObject();
            newLayer.AddComponent<Tilemap>();
            newLayer.AddComponent<TilemapRenderer>();

            newLayer.name = "Level (" + tilemapList3D.LayerCount() + ")";

            newLayer.transform.parent = grid.transform;
            float newHeightLevel = tilemapList3D.LayerCount() * grid.cellSize.y * grid.transform.localScale.y;
            newLayer.transform.position = new Vector3(0, newHeightLevel, 0);
            newLayer.transform.localScale = Vector3.one;

            tilemapList3D.AddLayer(newLayer);
            Debug.Log($"Add new Level: {newLayer.name}");
        }
    }

    private void CombineChunkOfBlock(GameObject gridObject)
    {
        ChunkList3D chunkList3D = gridObject.GetComponent<ChunkList3D>();
        if (chunkList3D == null)
        {
            Debug.Log("Require ChunkList3D Component inside the Grid");
            return;
        }

        GetAllChunkBlock(gridObject, out Dictionary<(int, int, int), Chunk> chunks);

        foreach (var kvp in chunks)
        {
            Chunk chunk = kvp.Value;
            GameObject combinedMeshObject = chunk.CombineBlockChunk();
            combinedMeshObject.transform.position = chunk.startPoint;
            combinedMeshObject.name = "Chunk" + kvp.Key;
            chunkList3D.AddChunk(combinedMeshObject);
        }
    }

    private void SavaBinaryData(GameObject gridObject)
    {
        GetAllChunkBlock(gridObject, out Dictionary<(int, int, int), Chunk> chunks);
        SaveSystem saveSystem = gridObject.GetComponent<SaveSystem>();
        saveSystem.SerializeBlockData(chunks);
        Debug.Log("Save Binary Data Success!");
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
    #endregion

    #region simplify function
    private void SetRecordChunkPos(Vector3Int blockPosition, out int x, out int y, out int z)
    {
        x = Mathf.FloorToInt(blockPosition.x / Chunk.CHUNK_SIZE);
        y = Mathf.FloorToInt(blockPosition.y / Chunk.CHUNK_SIZE);
        z = Mathf.FloorToInt(blockPosition.z / Chunk.CHUNK_SIZE);
    }
    private void GetAllChunkBlock(GameObject gridObject, out Dictionary<(int, int, int), Chunk> chunks)
    {
        Grid grid = gridObject.GetComponent<Grid>();
        List<Transform> allLayer = new List<Transform>();
        Vector3 offset = Vector3.one * 0.5f;

        chunks = new Dictionary<(int, int, int), Chunk>();

        foreach (Transform layer in grid.transform)
        {
            allLayer.Add(layer);
        }


        for (int i = 0; i < allLayer.Count; i++)
        {
            Transform parentTransform = allLayer[i];
            foreach (Transform child in parentTransform)
            {
                Vector3Int childPos = Vector3Int.FloorToInt(child.position + offset);
                SetRecordChunkPos(childPos, out int x, out int y, out int z);
                if (!chunks.TryGetValue((x, y, z), out Chunk chunk))
                {
                    chunk = new Chunk(x, y, z);
                    chunks.Add((x, y, z), chunk);
                }
                chunk.AddBlock(childPos, child.gameObject);
                if (hideBlockMergeToggle) { child.gameObject.SetActive(false); }
            }
        }
    }
    #endregion
}