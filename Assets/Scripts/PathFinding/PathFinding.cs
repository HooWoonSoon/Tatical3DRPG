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
    private List<GameNode> FindPath(int startWorldX, int startWorldY, int startWorldZ, int endWorldX, int endWorldY, int endWorldZ)
    {
        float startTime = Time.realtimeSinceStartup;
        List<GameNode> ret = new List<GameNode>();

        GameNode startNode = world.GetNode(startWorldX, startWorldY, startWorldZ);
        GameNode endNode = world.GetNode(endWorldX, endWorldY, endWorldZ);

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
                //Debug.Log($"Find path completed in {endTime - startTime:F4} seconds");
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
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int zDistance = Mathf.Abs(a.z - b.z);
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
        return world.GetNode(x, y, z);
    }

    private List<GameNode> GetNeighbourList(GameNode currentNode)
    {
        List<GameNode> neighbourList = new List<GameNode>();

        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(-1, 0, 0), // Left
            new Vector3Int(1, 0, 0),  // Right
            new Vector3Int(0, 0, -1), // Back
            new Vector3Int(0, 0, 1),  // Forward
            new Vector3Int(0, 1, 0),  // Up
            new Vector3Int(0, -1, 0)  // Down
        };

        foreach (var direction in directions)
        {
            Vector3Int neighbourPos = new Vector3Int(currentNode.x, currentNode.y, currentNode.z) + direction;
            if (world.loadedNodes.TryGetValue(neighbourPos, out GameNode neighbourNode))
            {
                neighbourList.Add(neighbourNode);
            }
        }
        return neighbourList;
    }

    public void SetProcessPath(Vector3 currentPosition, Vector3 targetPosition)
    {
        float startTime = Time.realtimeSinceStartup;

        world.GetWorldPosition(targetPosition, out int endX, out int endY, out int endZ);
        world.GetWorldPosition(currentPosition, out int startX, out int startY, out int startZ);

        if (!world.IsValidNode(startX, startY, startZ) || !world.IsValidNode(endX, endY, endZ))
        {
            Debug.Log("Invalid node position");
            return;
        }

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