using System;
using System.Collections.Generic;
using UnityEngine;
public class CharacterMoveTesting : MonoBehaviour
{
    public LayerMask LayerMask;
    private World world;
    private PathFinding pathFinding;

    private List<Vector3> pathVectorList;
    private int pathIndex = -1;

    public float moveSpeed = 5f;
    private float reachPathPositionDistance = 0.5f;
    private Action onReachedTargetPosition;

    public void Start()
    {
        world = WorldManager.instance.world;
        pathFinding = new PathFinding(world);
    }

    public void SetMovePosition(Vector3 movePosition, Action onReachedTargetPosition)
    {
        this.onReachedTargetPosition = onReachedTargetPosition;
        pathVectorList = pathFinding.GetPathRoute(transform.position, movePosition).pathVectorList;
        if (pathVectorList.Count > 0)
        {
            pathIndex = 0;
        }
        else
        {
            pathIndex = -1;
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 targetPosition = Utils.GetDDAWorldPosition(64, world.loadedNodes);
            if (targetPosition == new Vector3Int(-1, -1, -1)) return;

            SetMovePosition(targetPosition, null);
        }

        if (pathIndex != -1)
        {
            Vector3 nextPathPosition = pathVectorList[pathIndex];
            Vector3 moveDirection = (nextPathPosition - transform.position).normalized;

            transform.position += moveDirection * moveSpeed * Time.deltaTime;
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
