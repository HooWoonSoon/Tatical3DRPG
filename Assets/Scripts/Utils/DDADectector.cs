using System.Collections.Generic;
using UnityEngine;

public static class DDADectector
{
    public static bool CheckCubeAtPosition(Vector3 position, Dictionary<(int, int, int), GameNode> loadedNodes)
    {
        Vector3Int blockPosition = Vector3Int.RoundToInt(position);
        return loadedNodes.TryGetValue((blockPosition.x, blockPosition.y, blockPosition.z), out GameNode node) && node.hasNode;
    }

    #region DDA Algorithms by calculate the rasterization of the ray
    private static (Vector3Int blockPosition, Vector3Int step, Vector3 tMax, Vector3 tDelta) InitializeDDA(Vector3 startPosition, Vector3 direction)
    {
        Vector3Int blockPosition = Vector3Int.RoundToInt(startPosition);

        Vector3Int step = new Vector3Int(
            direction.x > 0 ? 1 : -1,
            direction.y > 0 ? 1 : -1,
            direction.z > 0 ? 1 : -1);

        Vector3 tMax = new Vector3(
            direction.x != 0 ? (blockPosition.x + (step.x > 0 ? 1 : 0) - startPosition.x) / direction.x : Mathf.Infinity,
            direction.y != 0 ? (blockPosition.y + (step.y > 0 ? 1 : 0) - startPosition.y) / direction.y : Mathf.Infinity,
            direction.z != 0 ? (blockPosition.z + (step.z > 0 ? 1 : 0) - startPosition.z) / direction.z : Mathf.Infinity);

        Vector3 tDelta = new Vector3(
            direction.x != 0 ? Mathf.Abs(1 / direction.x) : Mathf.Infinity,
            direction.y != 0 ? Mathf.Abs(1 / direction.y) : Mathf.Infinity,
            direction.z != 0 ? Mathf.Abs(1 / direction.z) : Mathf.Infinity);

        return (blockPosition, step, tMax, tDelta);
    }

    public static bool DDARaycast(Vector3 startPosition, Vector3 direction, int maxDistance, Dictionary<(int, int, int), GameNode> loadedNodes, 
        out Vector3Int? cubePosition)
    {
        cubePosition = null;

        if (direction == Vector3.zero) return false;

        var (currentDDAblock, step, tMax, tDelta) = InitializeDDA(startPosition, direction);

        if (loadedNodes.TryGetValue((currentDDAblock.x, currentDDAblock.y, currentDDAblock.z), out GameNode startNode))
        {
            if (startNode.hasNode)
            {
                cubePosition = currentDDAblock;
                return true;
            }
        }

        for (int i = 0; i < maxDistance; i++)
        {
            if (tMax.x < tMax.y)
            {
                if (tMax.x < tMax.z)
                {
                    currentDDAblock.x += step.x;
                    tMax.x += tDelta.x;
                }
                else
                {
                    currentDDAblock.z += step.z;
                    tMax.z += tDelta.z;
                }
            }
            else
            {
                if (tMax.y < tMax.z)
                {
                    currentDDAblock.y += step.y;
                    tMax.y += tDelta.y;
                }
                else
                {
                    currentDDAblock.z += step.z;
                    tMax.z += tDelta.z;
                }
            }

            Debug.DrawLine(startPosition, currentDDAblock, Color.green);

            if (loadedNodes.TryGetValue((currentDDAblock.x, currentDDAblock.y, currentDDAblock.z), out GameNode node))
            {
                if (node.hasNode)
                {
                    cubePosition = currentDDAblock;
                    return true;
                }
            }
        }
        return false;
    }
    public static Vector3Int GetDDAWorldPosition(int maxDistance, Dictionary<(int, int, int), GameNode> loadedNodes)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 startPosition = ray.origin;
        Vector3 direction = ray.direction;
        Debug.DrawRay(startPosition, direction * 64, Color.red);

        return DDAAlgorithms(startPosition, direction, maxDistance, loadedNodes);
    }

    public static Vector3Int DDAAlgorithms(Vector3 startPosition, Vector3 direction, int maxDistance, Dictionary<(int, int, int), GameNode> loadedNodes)
    {
        var (blockPosition, step, tMax, tDelta) = InitializeDDA(startPosition, direction);

        if (loadedNodes.TryGetValue((blockPosition.x, blockPosition.y, blockPosition.z), out GameNode startNode))
        {
            if (startNode.hasNode)
            {
                return blockPosition;
            }
        }

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

            if (loadedNodes.TryGetValue((blockPosition.x, blockPosition.y, blockPosition.z), out GameNode node))
            {
                if (node.hasNode)
                {
                    return blockPosition;
                }
            }
        }
        return new Vector3Int(-1, -1, -1);
    }
    #endregion
}