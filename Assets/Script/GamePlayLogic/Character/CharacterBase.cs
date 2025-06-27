using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterBase : Entity
{
    public enum Orientation
    {
        right, left, forward, back
    }

    public TeamDeployment currentTeam;

    [Header("Character Information")]
    public CharacterData data;
    public GameObject imageObject;
    public int currenthealth;
    public int index { get; set; }

    #region self path
    public readonly List<Vector3> positionHistory = new List<Vector3>();
    public int historyLimit { get; set; }

    private float recordInterval = 0.05f;
    private float recordTimer = 0f;
    #endregion

    #region state
    public bool isLeader;
    public bool isSelected;
    public bool isLink;
    public bool isBusy;

    public bool isExplore;
    public bool isBattle;

    public bool isMoving;
    public Vector3? targetPosition = null;
    public Orientation orientation = Orientation.right;
    #endregion

    public StateMachine stateMachine;
    public EventDriver eventDriver;
    public UnitDetectable detectable;
    private Animator anim;

    public BattleState battleState { get; private set; }
    public ExplorationState explorationState { get; private set; }

    public IdleStateExplore idleStateExplore { get; private set; }
    public MoveStateExplore moveStateExplore { get; private set; }
    public MovePathStateExplore movePathStateExplore { get; private set; }
    public IdleStateBattle idleStateBattle { get; private set; }

    [Header("Physic")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 9.8f;
    private float velocity;

    public float xInput { get; private set; }
    public float zInput { get; private set; }

    public PathRoute pathRoute;

    private void Awake()
    {
        stateMachine = new StateMachine();
        eventDriver = new EventDriver();

        explorationState = new ExplorationState(stateMachine, this);
        battleState = new BattleState(stateMachine, this);

        idleStateExplore = new IdleStateExplore(stateMachine, this);
        moveStateExplore = new MoveStateExplore(stateMachine, this);
        movePathStateExplore = new MovePathStateExplore(stateMachine, this);
        idleStateBattle = new IdleStateBattle(stateMachine, this);
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        detectable = GetComponent<UnitDetectable>();
        stateMachine.Initialize(explorationState, idleStateExplore);
        eventDriver.Subscribe("BattleMode", () => stateMachine.Initialize(battleState, idleStateBattle));
        eventDriver.Subscribe("ExplorationMode", () => stateMachine.Initialize(explorationState, idleStateExplore));
    }

    private void Update()
    {
        stateMachine.roofState.Update();
        stateMachine.subState.Update();
    }

    public void UpdateHistory()
    {
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;

            if (positionHistory.Count >= historyLimit)
                positionHistory.RemoveAt(0);

            positionHistory.Add(this.transform.position);
        }
    }
    public void CleanAllHistory()
    {
        positionHistory.Clear();
    }
    public void SetVelocity(float xInput, float zInput)
    {
        this.xInput = xInput;
        this.zInput = zInput;
    }
    public void FacingDirection(Vector3 direction)
    {
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
    public void UpdateOrientation(Vector3 direction)
    {
        direction = Vector3Int.RoundToInt(direction);

        if (direction == new Vector3(-1, 0, 0))
        {
            orientation = Orientation.left;
        }
        else if (direction == new Vector3(1, 0, 0))
        {
            orientation = Orientation.right;
        }
        else if (direction == new Vector3(0, 0, -1))
        {
            orientation = Orientation.back;
        }
        else if (direction == new Vector3(0, 0, 1))
        {
            orientation = Orientation.forward;
        }
    }
    public Vector3Int GetOrientationVector()
    {
        switch (orientation)
        {
            case Orientation.left:
                return new Vector3Int(-1, 0, 0);
            case Orientation.right:
                return new Vector3Int(1, 0, 0);
            case Orientation.back:
                return new Vector3Int(0, 0, -1);
            case Orientation.forward:
                return new Vector3Int(0, 0, 1);
            default:
                return Vector3Int.zero;
        }
    }

    //  Summary
    //      Move the unit character with the frequence input
    public void Move(float x, float z)
    {
        if (isBattle) { return; }

        if (x == 0 && z == 0)
        {
            //  No movement input
            isMoving = false;
            return;
        }

        Vector3 direction = new Vector3(x, 0, z).normalized;
        Vector3 characterPosition = transform.position;
        isMoving = true;
        FacingDirection(direction);
        UpdateOrientation(direction);

        //  Check diagonol movement and return
        Vector3 targetPosition = characterPosition + direction * moveSpeed * Time.deltaTime;
        if (world.IsValidNode(targetPosition))
        {
            transform.position = targetPosition;
            return;
        }

        //  Check X axis movement and return
        Vector3 targetPositionX = characterPosition + new Vector3(direction.x, 0, 0) * moveSpeed * Time.deltaTime;
        if (world.IsValidNode(targetPositionX))
        {
            transform.position = targetPositionX;
            return;
        }

        //  Check Z axis movement and return
        Vector3 targetPositionZ = characterPosition + new Vector3(0, 0, direction.z) * moveSpeed * Time.deltaTime;
        if (world.IsValidNode(targetPositionZ))
        {
            transform.position = targetPositionZ;
            return;
        }
    }

    #region UpDownHill
    public void UpDownHill(float xInput, float zInput)
    {
        DownHill();
        UpHill(xInput, zInput);

        if (targetPosition != null)
        {
            float currentX = Mathf.Lerp(transform.position.x, targetPosition.Value.x, Time.deltaTime);
            float currentY = Mathf.Lerp(transform.position.y, targetPosition.Value.y, Time.deltaTime * 10);
            float currentZ = Mathf.Lerp(transform.position.z, targetPosition.Value.z, Time.deltaTime);
            transform.position = new Vector3(currentX, currentY, currentZ);

            if (Mathf.Abs(transform.position.y - targetPosition.Value.y) < 0.1f)
            {
                transform.position = new(transform.position.x, targetPosition.Value.y, transform.position.z);
                targetPosition = null;
                isBusy = false;
            }
        }
    }
    private void DownHill()
    {
        if (DDADectector.DDARaycast(transform.position, Vector3.down, 16, world.loadedNodes,
            out Vector3Int? cubePosition))
        {
            int distance = Mathf.FloorToInt(transform.position.y - cubePosition.Value.y);
            //Debug.Log(isBusy);
            if (!isBusy)
            {
                if (distance == 1)
                {
                    if (cubePosition != null)
                    {
                        targetPosition = transform.position - new Vector3(0, 1, 0);
                        isBusy = true;
                    }
                }
            }
        }
    }
    private void UpHill(float xInput, float zInput)
    {
        Vector3Int direction = Utils.GetInputDirection(xInput, zInput);
        if (DDADectector.DDARaycast(transform.position, direction, 1, world.loadedNodes,
            out Vector3Int? cubePosition))
        {
            if (!isBusy)
            {
                Debug.Log(cubePosition);
                float distance1 = Vector3.Distance(transform.position, cubePosition.Value);
                Debug.Log(distance1);
                if (distance1 < 0.8f)
                {
                    targetPosition = transform.position + new Vector3(0, 1, 0) + direction;
                    isBusy = true;
                }
            }
        }
    }
    #endregion

    private void Drop()
    {
        //Debug.Log(Utils.CheckCubeAtPosition(unitCharacter.transform.position, world.loadedNodes));

        velocity += gravity * Time.deltaTime;
        transform.position -= new Vector3(0, 1 * velocity * Time.deltaTime, 0);

        if (DDADectector.CheckCubeAtPosition(transform.position, world.loadedNodes))
        {
            Vector3 alignedPosition = new Vector3(
                transform.position.x,
                Mathf.Floor(transform.position.y) + 0.5f,
                transform.position.z
            );
            transform.position = alignedPosition;

            velocity = 0;
        }
    }

    public void SetMovePosition(CharacterBase character, Vector3Int targetPosition)
    {
        List<Vector3> pathVectorList = pathFinding.GetPathRoute(character.transform.position, targetPosition).pathRouteList;
        if (pathVectorList.Count != 0)
        {
            pathRoute = new PathRoute
            {
                character = character,
                targetPosition = targetPosition,
                pathRouteList = pathVectorList,
                pathIndex = 0
            };
            stateMachine.ChangeSubState(movePathStateExplore);
        }
    }
    public void PathfindingMoveToTarget()
    {
        if (pathRoute == null) return;

        if (pathRoute.pathIndex != -1)
        {
            Vector3 nextPathPosition = pathRoute.pathRouteList[pathRoute.pathIndex];
            Vector3 currentPos = pathRoute.character.transform.position;
            Vector3 direction = (nextPathPosition - currentPos).normalized;

            FacingDirection(direction);
            pathRoute.character.transform.position = Vector3.MoveTowards(currentPos, nextPathPosition, 5 * Time.deltaTime);

            if (Vector3.Distance(pathRoute.character.transform.position, nextPathPosition) <= 0.1f)
            {
                pathRoute.character.transform.position = nextPathPosition;
                pathRoute.pathIndex++;
                if (pathRoute.pathIndex >= pathRoute.pathRouteList.Count)
                {
                    Debug.Log($"Reached target {pathRoute.targetPosition}");
                    pathRoute.pathIndex = -1;
                    pathRoute = null;
                }
            }
        }
    }
    public void TriggerBattle()
    {
        eventDriver.Trigger("BattleMode");
    }

    public Vector3Int GetCharacterPosition()
    {
        return Utils.RoundXZFloorYInt(transform.position);
    }
}