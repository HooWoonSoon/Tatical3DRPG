using System;
using TMPro;
using UnityEngine;

public class TeamMovementControllerE : MonoBehaviour
{
    private World world;
    private TeamFollowSystem teamFollowSystem;

    [Header("Physicc")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 9.8f;
    private float velocity;

    private Vector3? targetPosition;
    private bool isBusy = false;
    private bool isMoving = false;

    public static event Action<bool> OnMovementStatusChanged;

    private void Start()
    {
        world = WorldManager.instance.world;
        teamFollowSystem = TeamFollowSystem.instance;
    }

    private void Update()
    {
        for (int i = 0; i < teamFollowSystem.teamFollowers.Count; i++)
        {
            UnitCharacter unitCharacter = teamFollowSystem.teamFollowers[i].unitCharacter;
            if (unitCharacter.isLeader == true)
            {
                InputController.GetMovementInput(out float inputX, out float inputZ);
                Move(inputX, inputZ, unitCharacter);

                UpDownHill(inputX, inputZ, unitCharacter);
            }
        }
    }

    private void Move(float x, float z, UnitCharacter unitCharacter)
    {
        Vector3 direction = new Vector3(x, 0, z).normalized;

        if (direction.magnitude > 0f)
        {
            isMoving = true;
            unitCharacter.FacingDirection(direction);

            Vector3 targetPosition = unitCharacter.transform.position + direction * moveSpeed * Time.deltaTime;
            if (world.worldMinX < targetPosition.x && world.worldMinY < targetPosition.y && world.worldMinZ < targetPosition.z
                && world.worldMaxX > targetPosition.x && world.worldMaxY > targetPosition.y && world.worldMaxZ > targetPosition.z)
            {
                unitCharacter.transform.position = targetPosition;
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                OnMovementStatusChanged?.Invoke(false);
            }
        }

        if (isMoving)
        {
            OnMovementStatusChanged?.Invoke(true);
        }
    }

    #region UpDownHill
    private void UpDownHill(float xInput, float zInput, UnitCharacter unitCharacter)
    {
        DownHill(unitCharacter);
        UpHill(xInput, zInput, unitCharacter);

        if (targetPosition != null)
        {
            float currentX = Mathf.Lerp(unitCharacter.transform.position.x, targetPosition.Value.x, Time.deltaTime);
            float currentY = Mathf.Lerp(unitCharacter.transform.position.y, targetPosition.Value.y, Time.deltaTime * 10);
            float currentZ = Mathf.Lerp(unitCharacter.transform.position.z, targetPosition.Value.z, Time.deltaTime);
            unitCharacter.transform.position = new Vector3(currentX, currentY, currentZ);

            if (Mathf.Abs(transform.position.y - targetPosition.Value.y) < 0.1f)
            {
                unitCharacter.transform.position = new(unitCharacter.transform.position.x, targetPosition.Value.y, transform.position.z);
                targetPosition = null;
                isBusy = false;
            }
        }
    }

    private void DownHill(UnitCharacter unitCharacter)
    {
        if (DDADectector.DDARaycast(unitCharacter.transform.position, Vector3.down, 16, world.loadedNodes,
            out Vector3Int? cubePosition))
        {
            int distance = Mathf.FloorToInt(unitCharacter.transform.position.y - cubePosition.Value.y);
            //Debug.Log(isBusy);
            if (!isBusy)
            {
                if (distance == 1)
                {
                    if (cubePosition != null)
                    {
                        targetPosition = unitCharacter.transform.position - new Vector3(0, 1, 0);
                        isBusy = true;
                    }
                }
            }
        }
    }

    private void UpHill(float xInput, float zInput, UnitCharacter unitCharacter)
    {
        Vector3Int direction = Utils.GetInputDirection(xInput, zInput);
        if (DDADectector.DDARaycast(unitCharacter.transform.position, direction, 1, world.loadedNodes,
            out Vector3Int? cubePosition))
        {
            if (!isBusy)
            {
                Debug.Log(cubePosition);
                float distance1 = Vector3.Distance(unitCharacter.transform.position, cubePosition.Value);
                Debug.Log(distance1);
                if (distance1 < 0.8f)
                {
                    targetPosition = unitCharacter.transform.position + new Vector3(0, 1, 0) + direction;
                    isBusy = true;
                }
            }
        }
    }
    #endregion

    private void Drop(UnitCharacter unitCharacter)
    {
        //Debug.Log(Utils.CheckCubeAtPosition(transform.position, world.loadedNodes));

        velocity += gravity * Time.deltaTime;
        unitCharacter.transform.position -= new Vector3(0, 1 * velocity * Time.deltaTime, 0);

        if (DDADectector.CheckCubeAtPosition(unitCharacter.transform.position, world.loadedNodes))
        {
            Vector3 alignedPosition = new Vector3(
                unitCharacter.transform.position.x,
                Mathf.Floor(unitCharacter.transform.position.y) + 0.5f,
                unitCharacter.transform.position.z
            );
            unitCharacter.transform.position = alignedPosition;

            velocity = 0;
        }
    }
}