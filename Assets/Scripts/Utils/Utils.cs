using System.Collections;
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

    public static TextMeshProUGUI CreateCanvasText(string text, Transform canvas, Vector3 localPosition, Quaternion quaternion, int fontSize, Color color, TextAlignmentOptions textAlignment)
    {
        GameObject gameObejct = new GameObject("Text", typeof(TextMeshProUGUI));
        Transform transform = gameObejct.transform;
        transform.SetParent(canvas, false);
        transform.localPosition = localPosition;
        transform.rotation = quaternion;

        TextMeshProUGUI textMeshPro = gameObejct.GetComponent<TextMeshProUGUI>();
        textMeshPro.text = text;
        textMeshPro.fontSize = fontSize;
        textMeshPro.color = color;
        textMeshPro.fontStyle = FontStyles.Bold;
        textMeshPro.alignment = textAlignment;
        textMeshPro.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        textMeshPro.fontSharedMaterial.SetColor("_OutlineColor", Color.black);
        textMeshPro.fontSharedMaterial.SetFloat("_OutlineWidth", 0.15f);
        textMeshPro.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        textMeshPro.fontSharedMaterial.SetColor("_UnderlayColor", Color.black);
        textMeshPro.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 0.5f);
        textMeshPro.fontSharedMaterial.SetFloat("_UnderlayOffsetY", 0f);
        textMeshPro.fontSharedMaterial.SetFloat("_UnderlayDilate", 0.2f);
        textMeshPro.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0f);

        return textMeshPro;
    }
    public static TextMeshPro CreateWorldText(string text, Vector3 localPosition, Quaternion quaternion, int fontSize, Color color, TextAlignmentOptions textAlignment, int sortingOrder = 0)
    {
        GameObject parent = GameObject.Find("World_Text_Parent");
        if (parent == null)
        { 
            parent = new GameObject("World_Text_Parent");
            parent.transform.position = Vector3.zero;
        }
        GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent.transform, false);
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
        if (EventSystem.current == null) return null;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();

        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count == 0) return null;

        foreach (RaycastResult result in results)
        {
            ICanvasRaycastFilter raycastFilter = result.gameObject.GetComponent<ICanvasRaycastFilter>();
            if (raycastFilter == null || raycastFilter.IsRaycastLocationValid(Input.mousePosition, Camera.main))
            {
                return result.gameObject;
            }
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
    public static Vector3Int GetRaycastHitNodePositionWithCollider(LayerMask layerMask, Dictionary<Vector3Int, GameNode> loadedNodes)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 64f, layerMask))
        {
            Vector3 hitPoint = hitInfo.point;
            Vector3Int blockPos = RoundXZFloorYInt(hitPoint);

            if (loadedNodes.TryGetValue((blockPos), out GameNode node))
            {
                return blockPos;
            }
        }

        return new Vector3Int(-1, -1, -1);
    }
    public static GameNode GetRaycastHitNode(Dictionary<Vector3Int, GameNode> loadedNodes)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 position = ray.origin;
        Vector3 direction = ray.direction.normalized;

        for (int i = 0; i < 2000; i++)
        {
            position += direction * 0.1f;
            Vector3Int blockPos = Vector3Int.FloorToInt(new Vector3(position.x, position.y, position.z));

            if (loadedNodes.TryGetValue(blockPos, out GameNode node))
            {
                //Debug.Log($"raycast hit node at {blockPos}");
                return node;
            }
        }
        return null;
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

    public static Vector3Int RoundXZFloorYInt(Vector3 position)
    {
        return new Vector3Int(
        Mathf.RoundToInt(position.x),
        Mathf.FloorToInt(position.y),
        Mathf.RoundToInt(position.z));
    }

    public static void RoundXZFloorYInt(Vector3 position, out int x, out int y, out int z)
    {
        x = Mathf.RoundToInt(position.x);
        y = Mathf.FloorToInt(position.y);
        z = Mathf.RoundToInt(position.z);
    }


    /// <summary>
    /// Sort the followerVectorRange based on distance to unit position
    /// </summary>
    public static List<Vector3Int> SortTargetRangeByDistance(Vector3Int from, List<Vector3Int> targets)
    {
        var sorted = new List<Vector3Int>(targets);
        sorted.Sort((a, b) => Vector3.Distance(from, a).CompareTo(Vector3.Distance(from, b)));
        return sorted;
    }

    public static IEnumerator UIExtraMoveCoroutine(RectTransform rectTransform, Vector2 extra, float duration)
    {
        Vector2 start = rectTransform.anchoredPosition;
        Vector2 end = rectTransform.anchoredPosition += extra;
        yield return UIMoveCoroutine(rectTransform, start, end, duration);
    }
    public static IEnumerator UIMoveCoroutine(RectTransform rectTransform, Vector2 start, Vector2 end, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
        rectTransform.anchoredPosition = end;
    }

    public static IEnumerator UIExtraScaleCoroutine(RectTransform rectTransform, Vector2 extra, float duration)
    {
        Vector2 start = rectTransform.sizeDelta;
        Vector2 end = rectTransform.sizeDelta += extra;
        yield return UIScaleCorroutine(rectTransform, start, end, duration);
    }
    public static IEnumerator UIScaleCorroutine(RectTransform rectTransform, Vector2 start, Vector2 end, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            rectTransform.sizeDelta = Vector2.Lerp(start, end, t);
            yield return null;
        }
        rectTransform.sizeDelta = end;
    }
    public static IEnumerator UIColorInverseCorroutine(Image image, float duration)
    {
        Color startColor = image.color;
        Color endColor = new Color(1f - startColor.r, 1f - startColor.g, 1f - startColor.b, startColor.a);
        yield return UIColorCorroutine(image, startColor, endColor, duration);
    }

    public static IEnumerator UIColorCorroutine(Image image, Color endColor, float duration)
    {
        Color startColor = image.color;
        yield return UIColorCorroutine(image, startColor, endColor, duration);
    }

    public static IEnumerator UIColorCorroutine(Image image, Color startColor, Color endColor, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            Color color = Color.Lerp(startColor, endColor, t);
            image.color = color;
            yield return null;
        }
        image.color = endColor;
    }

    public static IEnumerator TextColorInverseCorroutine(TextMeshProUGUI text, float duration)
    {
        Color startColor = text.color;
        Color endColor = new Color(1f - startColor.r, 1f - startColor.g, 1f - startColor.b, startColor.a);
        yield return TextColorCoroutine(text, startColor, endColor, duration);
    }

    public static IEnumerator TextColorCoroutine(TextMeshProUGUI text, Color startColor, Color endColor, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            Color color = Color.Lerp(startColor, endColor, t);
            text.color = color;
            yield return null;
        }
        text.color = endColor;
    }
}
