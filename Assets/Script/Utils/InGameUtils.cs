using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    #region DDA Algorithms by calculate the rasterization of the ray
    public static Vector3Int GetDDAWorldPosition(int maxDistance, Dictionary<(int, int, int), GameNode> loadedNodes)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 startPosition = ray.origin;
        Vector3 direction = ray.direction;

        return DDAAlgorithms(startPosition, direction, maxDistance, loadedNodes);
    }

    public static Vector3Int DDAAlgorithms(Vector3 startPosition, Vector3 direction, int maxDistance, Dictionary<(int, int, int), GameNode> loadedNodes)
    {
        Vector3Int blockPosition = Vector3Int.FloorToInt(startPosition);

        Vector3Int step = new Vector3Int(
            direction.x > 0 ? 1 : -1, 
            direction.y > 0 ? 1 : -1, 
            direction.z > 0 ? 1 : -1);

        Vector3 tMax = new Vector3(
            direction.x != 0 ? (blockPosition.x + (step.x > 0 ? 1: 0) - startPosition.x) / direction.x : Mathf.Infinity,
            direction.y != 0 ? (blockPosition.y + (step.y > 0 ? 1 : 0) - startPosition.y) / direction.y : Mathf.Infinity,
            direction.z != 0 ? (blockPosition.z + (step.z > 0 ? 1 : 0) - startPosition.z) / direction.z : Mathf.Infinity
            );

        Vector3 tDelta = new Vector3(
            direction.x != 0 ? Mathf.Abs(1 / direction.x) : Mathf.Infinity,
            direction.y != 0 ? Mathf.Abs(1 / direction.y) : Mathf.Infinity,
            direction.z != 0 ? Mathf.Abs(1 / direction.z) : Mathf.Infinity
        );

        Vector3Int previousPos = blockPosition;

        for (int i = 0; i < maxDistance; i++)
        {
            if (tMax.x < tMax.y)
            {
                if (tMax.x < tMax.z)
                {
                    blockPosition.x += step.x;
                    tMax.x += tDelta.x;
                }
                else
                {
                    blockPosition.z += step.z;
                    tMax.z += tDelta.z;
                }
            }
            else
            {
                if (tMax.y < tMax.z)
                {
                    blockPosition.y += step.y;
                    tMax.y += tDelta.y;
                }
                else
                {
                    blockPosition.z += step.z;
                    tMax.z += tDelta.z;
                }
            }

            if (loadedNodes.ContainsKey((blockPosition.x, blockPosition.y, blockPosition.z)))
            {
                if (loadedNodes[(blockPosition.x, blockPosition.y, blockPosition.z)].hasCube)
                {
                    return blockPosition;
                }
            }
        }
        return new Vector3Int(-1,-1,-1);
    }
    #endregion

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
}
