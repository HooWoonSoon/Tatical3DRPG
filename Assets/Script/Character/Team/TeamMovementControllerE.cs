using System;
using UnityEngine;

public class TeamMovementControllerE : MonoBehaviour
{
    private World world;
    private TeamFollowSystem teamFollowSystem;

    [Header("Physic")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 9.8f;
    private float velocity;

    public static TeamMovementControllerE instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        world = WorldManager.instance.world;
        teamFollowSystem = TeamFollowSystem.instance;
    }

    private void Update()
    {
        Utils.GetMovementInput(out float inputX, out float inputZ);
        
        UnitCharacter unitCharacter = teamFollowSystem.teamFollowers[0].unitCharacter;  //leader
        Move(inputX, inputZ, unitCharacter);
        UpDownHill(inputX, inputZ, unitCharacter);
    }

    public void Move(float x, float z, UnitCharacter unitCharacter)
    {
        Vector3 direction = new Vector3(x, 0, z).normalized;

        if (direction.magnitude > 0f)
        {
            unitCharacter.isMoving = true;
            unitCharacter.FacingDirection(direction);

            Vector3 targetPosition = unitCharacter.transform.position + direction * moveSpeed * Time.deltaTime;

            if (world.IsValidNode(targetPosition))
            {
                unitCharacter.transform.position = targetPosition;
            }
        }
        else
        {
            if (unitCharacter.isMoving)
            {
                unitCharacter.isMoving = false;
            }
        }
    }

    #region UpDownHill
    public void UpDownHill(float xInput, float zInput, UnitCharacter unitCharacter)
    {
        DownHill(unitCharacter);
        UpHill(xInput, zInput, unitCharacter);

        if (unitCharacter.targetPosition != null)
        {
            float currentX = Mathf.Lerp(unitCharacter.transform.position.x, unitCharacter.targetPosition.Value.x, Time.deltaTime);
            float currentY = Mathf.Lerp(unitCharacter.transform.position.y, unitCharacter.targetPosition.Value.y, Time.deltaTime * 10);
            float currentZ = Mathf.Lerp(unitCharacter.transform.position.z, unitCharacter.targetPosition.Value.z, Time.deltaTime);
            unitCharacter.transform.position = new Vector3(currentX, currentY, currentZ);

            if (Mathf.Abs(unitCharacter.transform.position.y - unitCharacter.targetPosition.Value.y) < 0.1f)
            {
                unitCharacter.transform.position = new(unitCharacter.transform.position.x, unitCharacter.targetPosition.Value.y, unitCharacter.transform.position.z);
                unitCharacter.targetPosition = null;
                unitCharacter.isBusy = false;
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
            if (!unitCharacter.isBusy)
            {
                if (distance == 1)
                {
                    if (cubePosition != null)
                    {
                        unitCharacter.targetPosition = unitCharacter.transform.position - new Vector3(0, 1, 0);
                        unitCharacter.isBusy = true;
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
            if (!unitCharacter.isBusy)
            {
                Debug.Log(cubePosition);
                float distance1 = Vector3.Distance(unitCharacter.transform.position, cubePosition.Value);
                Debug.Log(distance1);
                if (distance1 < 0.8f)
                {
                    unitCharacter.targetPosition = unitCharacter.transform.position + new Vector3(0, 1, 0) + direction;
                    unitCharacter.isBusy = true;
                }
            }
        }
    }
    #endregion

    private void Drop(UnitCharacter unitCharacter)
    {
        //Debug.Log(Utils.CheckCubeAtPosition(unitCharacter.transform.position, world.loadedNodes));

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

    public bool IsLeaderMove(UnitCharacter unitCharacter) => unitCharacter.isMoving;
}