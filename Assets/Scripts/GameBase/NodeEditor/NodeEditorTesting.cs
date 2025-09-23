using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodeEditorTesting : EditorWindow
{
    private List<MyNode> nodes;

    [MenuItem("Utils/Node Editor")]
    static void OpenWindow()
    {
        NodeEditorTesting window = GetWindow<NodeEditorTesting>("Node Editor");
        window.nodes = new List<MyNode>();
        window.nodes.Add(new MyNode(new Vector2(0, 0)));
    }

    private void OnGUI()
    {
        DrawNodes();

        if (GUI.changed)
            Repaint();
    }

    private void DrawNodes()
    {
        if (nodes == null) return;
        for (int i = 0; i < nodes.Count; i++)
            nodes[i].Draw();
    }

}
