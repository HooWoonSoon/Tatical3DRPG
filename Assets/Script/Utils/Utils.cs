using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class Utils
{
    public static void CreateEmptyMeshArrays(int quatCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[4 * quatCount];
        uvs = new Vector2[4 * quatCount];
        triangles = new int[6 * quatCount];
    }

    private static Quaternion[] cachedQuaternionEulerArr;
    private static void CacheQuaternionEuler()
    {
        if (cachedQuaternionEulerArr != null) return;
        cachedQuaternionEulerArr = new Quaternion[360];
        for (int i = 0; i < 360; i++)
        {
            cachedQuaternionEulerArr[i] = Quaternion.Euler(0, 0, i);
        }
    }
    private static Quaternion GetQuaternionEuler(float rotFloat)
    {
        int rot = Mathf.RoundToInt(rotFloat);
        rot = rot % 360;
        if (rot < 0) rot += 360;
        //if (rot >= 360) rot -= 360;
        if (cachedQuaternionEulerArr == null) CacheQuaternionEuler();
        return cachedQuaternionEulerArr[rot];
    }

    public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11)
    {
        //Relocate vertices
        int vIndex = index * 4;
        int vIndex0 = vIndex;
        int vIndex1 = vIndex + 1;
        int vIndex2 = vIndex + 2;
        int vIndex3 = vIndex + 3;

        baseSize *= .5f;

        Quaternion rotation = GetQuaternionEuler(rot);

        vertices[vIndex0] = pos + rotation * new Vector3(-baseSize.x, baseSize.y, baseSize.x);
        vertices[vIndex1] = pos + rotation * new Vector3(-baseSize.x, baseSize.y, -baseSize.x);
        vertices[vIndex2] = pos + rotation * new Vector3(baseSize.x, baseSize.y, -baseSize.x);
        vertices[vIndex3] = pos + rotation * new Vector3(baseSize.x, baseSize.y, baseSize.x);

        //Relocate UVs
        uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
        uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
        uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
        uvs[vIndex3] = new Vector2(uv11.x, uv11.y);

        //Create triangles
        int tIndex = index * 6;

        triangles[tIndex + 0] = vIndex0;
        triangles[tIndex + 1] = vIndex3;
        triangles[tIndex + 2] = vIndex1;

        triangles[tIndex + 3] = vIndex1;
        triangles[tIndex + 4] = vIndex3;
        triangles[tIndex + 5] = vIndex2;
    }
    
    public static TextMeshPro CreateWorldText(string text, Transform parent, Vector3 localPosition, Quaternion quaternion, int fontSize, Color color, TextAlignmentOptions textAlignment, int sortingOrder = 0)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.rotation = quaternion;

        TextMeshPro textMeshPro = gameObject.GetComponent<TextMeshPro>();
        textMeshPro.text = text;
        textMeshPro.fontSize = fontSize;
        textMeshPro.color = color;
        textMeshPro.alignment = textAlignment;

        textMeshPro.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

        return textMeshPro;
    }

    public static GameObject GetMouseOverUIElement(Canvas canvas)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            return results[0].gameObject; 
        }
        return null; 
    }

    public static GameObject GetLayerMouseGameObject(LayerMask objectMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, objectMask))
        {
            return hitInfo.collider.gameObject;
        }
        return null;
    }

    public static Vector3 GetLayerMouseWorldPosition(LayerMask gridMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, gridMask))
        {
            return hitInfo.point;
        }
        else
            return Vector3.zero;
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
        {
            return hitInfo.point;
        }
        else
            return Vector3.zero;
    }

    public static Vector3Int GetInputDirection(float xInput, float zInput)
    {
        if (xInput > 0) { return new(1, 0, 0); }
        else if (xInput < 0) { return new(-1, 0, 0); }
        else if (zInput > 0) { return new(0, 0, 1); }
        else if (zInput < 0) { return new(0, 0, -1); }
        else if (xInput > 0 && zInput > 0) { return new(1, 0, 1); }
        else if (xInput < 0 && zInput > 0) { return new(-1, 0, 1); }
        else if (xInput > 0 && zInput < 0) { return new(1, 0, -1); }
        else if (xInput < 0 && zInput < 0) { return new(-1, 0, -1); }
        else { return new(0, 0, 0); }
    }
}
