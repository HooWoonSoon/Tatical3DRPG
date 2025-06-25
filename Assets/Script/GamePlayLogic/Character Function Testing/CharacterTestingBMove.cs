using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterTestingBMove : MonoBehaviour 
{
    public LayerMask LayerMask;
    private World world;
    private PathFinding pathFinding;

    public Character unitCharacter;

    private List<Vector3> pathVectorList;
    private int pathIndex = -1;

    public float moveSpeed = 5f;
    private float reachPathPositionDistance = 0.1f;
    private Action onReachedTargetPosition;

    public void Start()
    {
        world = WorldGeneration.instance.world;
        pathFinding = new PathFinding(world);
    }

    public void SetMovePosition(Vector3 movePosition, Action onReachedTargetPosition)
    {
        this.onReachedTargetPosition = onReachedTargetPosition;
        pathVectorList = pathFinding.GetPathRoute(transform.position, movePosition).pathRouteList;
        string result = string.Join(",\n", pathVectorList.Select(v => $"({v.x}, {v.y}, {v.z})"));
        Debug.Log(result);

        if (pathVectorList.Count > 0)
        {
            pathIndex = 0;
        }
        else
        {
            Debug.Log("Not path found");
            pathIndex = -1;
        }
    }

    public void Update()
    {
        Vector3 currentPosition = DDADectector.GetDDAWorldPosition(64, world.loadedNodes);
        //Debug.Log(currentPosition);
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 targetPosition = currentPosition;
            if (targetPosition == new Vector3Int(-1, -1, -1)) return;
            Debug.Log(world.loadedNodes[((int)targetPosition.x, (int)targetPosition.y, (int)targetPosition.z)].isWalkable);

            SetMovePosition(targetPosition, null);
        }

        if (pathIndex != -1)
        {
            Vector3 nextPathPosition = pathVectorList[pathIndex];
            Vector3 direction = (nextPathPosition - transform.position).normalized;
            Debug.Log("moveDirection:" + direction);
            unitCharacter.FacingDirection(direction);

            transform.position = Vector3.MoveTowards(transform.position, nextPathPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, nextPathPosition) <= reachPathPositionDistance)
            {
                transform.position = nextPathPosition;
                pathIndex++;

                if (pathIndex >= pathVectorList.Count)
                {
                    pathIndex = -1;
                    onReachedTargetPosition?.Invoke();
                }
            }
        }
    }
}
