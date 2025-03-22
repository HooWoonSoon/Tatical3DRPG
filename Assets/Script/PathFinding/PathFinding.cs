using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding
{
    private World world;
    private PriorityQueue<GameNode> openQueue;
    private HashSet<GameNode> closedSet;
    private List<GameNode> processedPath;

    private Dictionary<GameNode, List<GameNode>> neighbourCache = new Dictionary<GameNode, List<GameNode>>();

    public PathFinding(World world)
    {
        this.world = world;
        openQueue = new PriorityQueue<GameNode>();
        closedSet = new HashSet<GameNode>();
    }

    //  Summary
    //      Example I passing the world position to the pathfinding function,
    //      even if I save every chunk as the local position, I may not need to convert the world position to the local position
    //      because the pathfinding receive the world position and just check for the grid world position without interaction with the chunk
    public List<GameNode> FindPath(int startWorldX, int startWorldY, int startWorldZ, int endWorldX, int endWorldY, int endWorldZ)
    {
        float startTime = Time.realtimeSinceStartup;

        //  Summary
        //      return the empty list if the start and end node is not accrossable
        List<GameNode> ret = new List<GameNode>();

        GameNode startNode = world.GetNodeAtWorldPosition(startWorldX, startWorldY, startWorldZ);
        GameNode endNode = world.GetNodeAtWorldPosition(endWorldX, endWorldY, endWorldZ);

        openQueue.Enqueue(startNode);
        closedSet.Clear();

        foreach (var pathNode in world.loadedNodes.Values.ToList())
        {
            pathNode.gCost = int.MaxValue;
            pathNode.CalculateFCost();
            pathNode.cameFromNode = null;
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openQueue.Count > 0)
        {
            GameNode currentNode = openQueue.Dequeue();
            if (currentNode == endNode)
            {
                float endTime = Time.realtimeSinceStartup;
                List<GameNode> path = CalculatePath(endNode);

                return path;
            }

            closedSet.Add(currentNode);

            foreach (GameNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedSet.Contains(neighbourNode)) continue;

                if (!neighbourNode.isWalkable)
                {
                    closedSet.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openQueue.Contains(neighbourNode))
                        openQueue.Enqueue(neighbourNode);
                }
            }
        }
        // Out of nodes on the openList 
        return ret;
    }

    //  Summary
    //      Calculate cost between two nodes by using Manhattan distance. In 3D space,
    //      it is better that diagonal distance pathfinding, because it is more faster
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

    //  Summary
    //      Get the neighbour nodes of the current node, one angle mean 1 times caluclation
    private List<GameNode> GetNeighbourList(GameNode currentNode)
    {
        if (neighbourCache.TryGetValue(currentNode, out var cachedNeighbours))
            return cachedNeighbours;

        List<GameNode> neighbourList = new List<GameNode>();

        if (currentNode.x - 1 >= world.worldMinX)
            // Left
            neighbourList.Add(world.GetNodeAtWorldPosition(currentNode.x - 1, currentNode.y, currentNode.z));
        if (currentNode.x + 1 <= world.worldMaxX)
            // Right
            neighbourList.Add(world.GetNodeAtWorldPosition(currentNode.x + 1, currentNode.y, currentNode.z));
        if (currentNode.y - 1 >= world.worldMinY)
            // Down
            neighbourList.Add(world.GetNodeAtWorldPosition(currentNode.x, currentNode.y - 1, currentNode.z));
        if (currentNode.y + 1 <= world.worldMaxY)
            // Up
            neighbourList.Add(world.GetNodeAtWorldPosition(currentNode.x, currentNode.y + 1, currentNode.z));
        if (currentNode.z - 1 >= world.worldMinZ)
            // Back
            neighbourList.Add(world.GetNodeAtWorldPosition(currentNode.x, currentNode.y, currentNode.z - 1));
        if (currentNode.z + 1 <= world.worldMaxZ)
            // Forward
            neighbourList.Add(world.GetNodeAtWorldPosition(currentNode.x, currentNode.y, currentNode.z + 1));

        neighbourCache[currentNode] = neighbourList;
        return neighbourList;
    }

    public void SetProcessPath(Vector3 currentPosition, Vector3 movePosition)
    {
        float startTime = Time.realtimeSinceStartup;

        world.GetWorldPosition(movePosition, out int endX, out int endY, out int endZ);
        int startX = Mathf.RoundToInt(currentPosition.x);
        int startY = Mathf.RoundToInt(currentPosition.y);
        int startZ = Mathf.RoundToInt(currentPosition.z);

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