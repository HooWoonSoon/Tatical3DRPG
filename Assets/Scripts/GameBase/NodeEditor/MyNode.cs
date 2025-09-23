using UnityEngine;

public class MyNode
{
    public Rect rect;

    public MyNode(Vector2 position)
    {
        rect = new Rect(position.x, position.y, 100, 50);
    }

    public void Draw()
    {
        GUI.Box(rect, "NodeTesting");
    }
}
