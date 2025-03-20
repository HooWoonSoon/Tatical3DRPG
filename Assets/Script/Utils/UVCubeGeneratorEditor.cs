using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using System.IO;

public class UVCubeMeshGenerator : EditorWindow
{
    private string filePath = Path.Combine(Application.dataPath, "UnwarpCube.fbx");

    [MenuItem("Utils/Generate UVwarp Cube")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<UVCubeMeshGenerator>();
    }

    void OnGUI()
    {
        filePath = EditorGUILayout.TextField("Save Path", filePath);

        if (GUILayout.Button("Generate and Save Cube"))
        {
            GenerateAndSaveCubeMesh();
            Debug.Log($"Exported FBX to: {filePath}");
        }
    }

    void GenerateAndSaveCubeMesh()
    {
        GameObject cubeObject = new GameObject();
        MeshFilter meshFilter = cubeObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateCubeMesh();
        cubeObject.AddComponent<MeshRenderer>();

        ExportModelOptions exportModelOptions = new ExportModelOptions();
        exportModelOptions.ExportFormat = ExportFormat.Binary;
        exportModelOptions.KeepInstances = false;

        ModelExporter.ExportObject(filePath, cubeObject, exportModelOptions);
        DestroyImmediate(cubeObject);
        AssetDatabase.Refresh();
    }

    Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();

        //  Summary
        //      Center the cube coordinate point
        //
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), //Top
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), //Down
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), //Forward
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), //Back
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f), //Left
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f)  //Right
        };

        int[] triangles = new int[]
        {
            0, 2, 1, 0, 3, 2,   // Top
            4, 5, 6, 4, 6, 7,   // Bottom
            8, 9, 10, 8, 10, 11,  // Front
            12, 14, 13, 12, 15, 14, // Back
            16, 17, 18, 16, 18, 19, // Left
            20, 22, 21, 20, 23, 22  // Right
        };

        //  Summary
        //      UV vector mapping for 3*3 grid, which mean split the normal shape to 9 piece
        //      but seperate every mesh with top, down forward, back, left, right.
        //      The concept of calculation example like:
        //      in Vector3 y = 1 is top, then the vector2 uv will get (0, 1) default
        //
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0f, 2f/3f), new Vector2(1f/3f, 2f/3f), new Vector2(1f/3f, 1f), new Vector2(0f, 1f), //top
            new Vector2(2f/3f, 2f/3f), new Vector2(1f, 2f/3f), new Vector2(1f, 1f), new Vector2(2f/3f, 1f), //down
            new Vector2(0f, 1f/3f), new Vector2(1f/3f, 1f/3f), new Vector2(1f/3f, 2f/3f), new Vector2(0f, 2f/3f), //forward
            new Vector2(1f/3f, 1f/3f), new Vector2(2f/3f, 1f/3f), new Vector2(2f/3f, 2f/3f), new Vector2(1f/3f, 2f/3f), //back
            new Vector2(2f/3f, 1f/3f), new Vector2(1f, 1f/3f), new Vector2(1f, 2f/3f), new Vector2(2f/3f, 2f/3f), //left
            new Vector2(0f, 0f), new Vector2(1f/3f, 0f), new Vector2(1f/3f, 1f/3f), new Vector2(0f, 1f/3f)  //right
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}
