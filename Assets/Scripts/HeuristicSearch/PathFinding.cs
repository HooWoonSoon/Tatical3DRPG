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

    /// <summary>
    /// Input the world position to the pathfinding function,
    /// the pathfinding receive the world position and just check for the grid world position without interaction with the chunk
    /// </summary>
    private List<GameNode> FindPath(int startWorldX, int startWorldY, int startWorldZ, int endWorldX, int endWorldY, int endWorldZ,
        int riseLimit, int lowerLimit)
    {
        float startTime = Time.realtimeSinceStartup;
        List<GameNode> ret = new List<GameNode>();

        GameNode startNode = world.GetNode(startWorldX, startWorldY, startWorldZ);
        GameNode endNode = world.GetNode(endWorldX, endWorldY, endWorldZ);

        openList = new List<GameNode> { startNode };
        closedList = new HashSet<GameNode>();

        foreach (GameNode pathNode in world.loadedNodes.Values.ToList())
        {
            if (!pathNode.isWalkable) { continue; }
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


            foreach (GameNode neighbourNode in GetNeighbourList(currentNode, riseLimit, lowerLimit))
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

    /// <summary>
    /// In the distance between a and b, first of all need to finding the most shorter distance of x, y, z line
    /// in order to calculate the distance cost, then found the most shorter distance of line.
    /// Note that the A* algorithm triggers this function once every time the cell is moved.
    /// </summary>
    private int CalculateDistanceCost(GameNode a, GameNode b)
    {
        int xCost = Mathf.Abs(b.x - a.x);
        int yCost = Mathf.Abs(b.y - a.y);
        int zCost = Mathf.Abs(b.z - a.z);
        return (xCost + yCost + zCost);
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

    private List<GameNode> GetNeighbourList(GameNode currentNode, int riseLimit, int lowerLimit)
    {
        List<GameNode> neighbourList = new List<GameNode>();

        List<Vector3Int> directions = new List<Vector3Int>
        {
            new Vector3Int(-1, 0, 0), // Left
            new Vector3Int(1, 0, 0),  // Right
            new Vector3Int(0, 0, -1), // Back
            new Vector3Int(0, 0, 1),  // Forward
            new Vector3Int(0, 1, 0),  // Up
            new Vector3Int(0, -1, 0), // Down
            new Vector3Int(1, 0, 1),  // Diagonal Forward-Right
            new Vector3Int(-1, 0, 1), // Diagonal Forward-Left
            new Vector3Int(1, 0, -1), // Diagonal Backward-Right
            new Vector3Int(-1, 0, -1) // Diagonal Backward-Left
        };

        for (int y = 1; y <= riseLimit; y++)
        {
            directions.AddRange(new[]
            {
            new Vector3Int(1, y, 0),
            new Vector3Int(-1, y, 0),
            new Vector3Int(0, y, 1),
            new Vector3Int(0, y, -1)
        });
        }

        for (int y = 1; y <= lowerLimit; y++)
        {
            directions.AddRange(new[]
            {
            new Vector3Int(1, -y, 0),
            new Vector3Int(-1, -y, 0),
            new Vector3Int(0, -y, 1),
            new Vector3Int(0, -y, -1)
        });
        }

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

    public void SetProcessPath(Vector3 currentPosition, Vector3 targetPosition, int riseLimit, int lowerLimit)
    {   
        float startTime = Time.realtimeSinceStartup;

        world.GetWorldPosition(targetPosition, out int endX, out int endY, out int endZ);
        world.GetWorldPosition(currentPosition, out int startX, out int startY, out int startZ);

        if (!world.IsValidNode(startX, startY, startZ) || !world.IsValidNode(endX, endY, endZ))
        {
            Debug.Log("Invalid node position");
            processedPath = null;
            return;
        }

        processedPath = FindPath(startX, startY, startZ, endX, endY, endZ, riseLimit, lowerLimit);

        float endTime = Time.realtimeSinceStartup;
        //Debug.Log($"Set process path completed in {endTime - startTime:F4} seconds");
    }
    public PathRoute GetPathRoute(Vector3 start, Vector3 end, int riseLimit, int lowerLimit)
    {
        SetProcessPath(start, end, riseLimit, lowerLimit);
        if (processedPath.Count == 0) { Debug.Log("No path"); return null; }
        //string pathLog = string.Join(" -> ", processedPath.ConvertAll(p => p.GetVector().ToString()));
        //Debug.Log(pathLog);
        return new PathRoute(processedPath, start);
    }

    #region Dijkstra Region Search

    /// <summary>
    /// Get the coverange from input position then use the dijkstra algorithm
    /// to calculate the cost of each node check if the cost is lower than the
    /// movable range cost then add to the result list
    /// </summary>
    public List<Vector3Int> GetCostDijkstraCoverangePos(Vector3 start, int movableRangeCost, int riseLimit, int lowerLimit)
    {
        List<Vector3Int > result = new List<Vector3Int>();
        List<GameNode> costNodes = GetCalculateDijkstraCost(start, riseLimit, lowerLimit);
        foreach (GameNode node in costNodes)
        {
            if (node.dijkstraCost <= movableRangeCost)
            {
                result.Add(new Vector3Int(node.x, node.y, node.z));
            }
        }
        return result;
    }
    public List<GameNode> GetCostDijkstraCoverangeNodes(Vector3 start, int movableRangeCost, int riseLimit, int lowerLimit)
    {
        List<GameNode> result = new List<GameNode>();
        List<GameNode> costNodes = GetCalculateDijkstraCost(start, riseLimit, lowerLimit);
        foreach (GameNode node in costNodes)
        {
            if (node.dijkstraCost <= movableRangeCost)
            {
                result.Add(node);
            }
        }
        return result;
    }

    /// <summary>
    /// Get the coverange gamenode from input position then use the dijkstra algorithm
    /// to calculate the cost of each node till all the walkable node is calculated
    /// or the cost is over the 200 limit
    /// </summary>
    public List<GameNode> GetCalculateDijkstraCost(Vector3 start, int riseLimit, int lowerLimit)
    {
        GameNode startNode = world.GetNode(start);
        if (startNode == null)
        {
            Debug.LogWarning("Invalid start node position");
            return new List<GameNode>();
        }
        foreach (GameNode gameNode in world.loadedNodes.Values.ToList())
        {
            if (!gameNode.isWalkable) { continue; }
            gameNode.dijkstraCost = int.MaxValue;
            gameNode.cameFromNode = null;
        }
        startNode.dijkstraCost = 0;

        List<GameNode> openList = new List<GameNode> { startNode };
        List<GameNode> calcualtedNode = new List<GameNode> { startNode };

        while (openList.Count > 0)
        {
            GameNode currentNode = openList[0];
            openList.RemoveAt(0);

            List<GameNode> neighbourNodes = GetNeighbourList(currentNode, riseLimit, lowerLimit);
            foreach (GameNode neighbourNode in neighbourNodes)
            {
                int tentativeGCost = currentNode.dijkstraCost + CalculateSlopeCost(currentNode, neighbourNode);

                //  Limit the searching range to avoid the long pathfinding time
                if (tentativeGCost > 200)
                    continue;

                if (tentativeGCost < neighbourNode.dijkstraCost)
                {
                    neighbourNode.dijkstraCost = tentativeGCost;
                    neighbourNode.cameFromNode = currentNode;
                    calcualtedNode.Add(neighbourNode);
                    if (!openList.Contains(neighbourNode))
                        openList.Add(neighbourNode);
                }
            }
        }
        return calcualtedNode;
    }

    private int CalculateSlopeCost(GameNode a, GameNode b)
    {
        int xCost = Mathf.Abs(b.x - a.x);
        int height = b.y - a.y;
        int zCost = Mathf.Abs(b.z - a.z);
        if (height > 0)
        {
            return height + xCost + zCost;
        }
        else
        {
            return xCost + zCost;
        }
    }
    #endregion
}