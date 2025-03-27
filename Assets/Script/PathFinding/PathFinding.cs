using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding
{
    private World world;
    private List<GameNode> openList;
    private HashSet<GameNode> closedList;
    private List<GameNode> processedPath;

    public PathFinding(World world)
    {
        this.world = world;
    }

    //  Summary
    //      Example I passing the world position to the pathfinding function,
    //      even if I save every chunk as the local position, I may not need to convert the world position to the local position
    //      because the pathfinding receive the world position and just check for the grid world position without interaction with the chunk
    public List<GameNode> FindPath(int startWorldX, int startWorldY, int startWorldZ, int endWorldX, int endWorldY, int endWorldZ)
    {
        float startTime = Time.realtimeSinceStartup;
        List<GameNode> ret = new List<GameNode>();

        GameNode startNode = world.GetNodeAtWorldPosition(startWorldX, startWorldY, startWorldZ);
        GameNode endNode = world.GetNodeAtWorldPosition(endWorldX, endWorldY, endWorldZ);

        openList = new List<GameNode> { startNode };
        closedList = new HashSet<GameNode>();

        foreach (var key in world.loadedNodes.Keys.ToList())
        {
            GameNode pathNode = world.loadedNodes[key];
            pathNode.gCost = int.MaxValue;
            pathNode.CalculateFCost();
            pathNode.cameFromNode = null;
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            GameNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                float endTime = Time.realtimeSinceStartup;
                Debug.Log($"Find path completed in {endTime - startTime:F4} seconds");
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);


            foreach (GameNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;

                if (!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                        openList.Add(neighbourNode);
                }
            }
        }
        // Out of nodes on the openList 
        return ret;
    }

    //  Summary
    //      In the distance between a and b, first of all need to finding the most shorter distance of x, y, z line
    //      in order to calculate the distance cost, then found the most shorter distance of line.
    //      Note that the A* algorithm triggers this function once every time the cell is moved.
    private int CalculateDistanceCost(GameNode a, GameNode b)
    {
        int xDistance = Mathf.Abs(a.worldX - b.worldX);
        int yDistance = Mathf.Abs(a.worldY - b.worldY);
        int zDistance = Mathf.Abs(a.worldZ - b.worldZ);
        return (xDistance + yDistance + zDistance);
    }

    private GameNode GetLowestFCostNode(List<GameNode> nodeList)
    {
        GameNode lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = nodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private List<GameNode> CalculatePath(GameNode endnode)
    {
        List<GameNode> path = new List<GameNode>();
        path.Add(endnode);
        GameNode currentNode = endnode;
        //  Summary
        //      Take the node from the previously saved cameFromNode to return and
        //      path points step by step and generate separate and complete total paths.
        //      Can think like gamenode cameFromNode inside have another gamenode till the start gamenode.cameFromNode is null
        while (currentNode.cameFromNode != null)
        {
            path.Insert(0, currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        return path;
    }

    private GameNode GetNode(int x, int y, int z)
    {
        return world.GetNodeAtWorldPosition(x, y, z);
    }

    private List<GameNode> GetNeighbourList(GameNode currentNode)
    {
        List<GameNode> neighbourList = new List<GameNode>();

        if (currentNode.x - 1 >= world.worldMinX)
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y, currentNode.z));
        if (currentNode.x + 1 < world.worldMaxX)
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y, currentNode.z));
        if (currentNode.y + 1 < world.worldMaxY)
            // Up
            neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1, currentNode.z));
        if (currentNode.y - 1 >= world.worldMinY)
            // Down
            neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1, currentNode.z));
        if (currentNode.z - 1 >= world.worldMinZ)
            // Back
            neighbourList.Add(GetNode(currentNode.x, currentNode.y, currentNode.z - 1));
        if (currentNode.z + 1 < world.worldMaxZ)
            // Forward
            neighbourList.Add(GetNode(currentNode.x, currentNode.y, currentNode.z + 1));

        return neighbourList;
    }

    public void SetProcessPath(Vector3 currentPosition, Vector3 movePosition)
    {
        float startTime = Time.realtimeSinceStartup;

        world.GetWorldPosition(movePosition, out int endX, out int endY, out int endZ);
        int startX = Mathf.FloorToInt(currentPosition.x);
        int startY = Mathf.FloorToInt(currentPosition.y);
        int startZ = Mathf.FloorToInt(currentPosition.z);

        processedPath = FindPath(startX, startY, startZ, endX, endY, endZ);

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"Set process path completed in {endTime - startTime:F4} seconds");
    }

    public PathRoute GetPathRoute(Vector3 start, Vector3 end)
    {
        SetProcessPath(start, end);
        return new PathRoute(processedPath, start);
    }
}