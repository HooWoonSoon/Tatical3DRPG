using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : CharacterBase
{
    public GameObject imageObject;
    public int index { get; set; }

    #region self path
    public readonly List<Vector3> positionHistory = new List<Vector3>();
    public int historyLimit { get; set; }

    private float recordInterval = 0.05f;
    private float recordTimer = 0f;
    #endregion

    #region state
    public bool isLeader;
    public bool isLink;
    public bool isBusy;
    public bool isMoving;
    public bool isComanding;
    #endregion

    public Vector3? targetPosition = null;

    public PlayerStateMachine stateMechine;
    private Animator anim;
    public PlayerExploreState exploreState { get; private set; }
    public PlayerReadyBattleState readyBattleState { get; private set; }
    public PlayerBattleState battleState { get; private set; }
    public PlayerWaitState waitState { get; private set; }

    public PlayerIdleStateExplore idleStateExplore { get; private set; }
    public PlayerMoveStateExplore moveStateExplore { get; private set; }
    public PlayerMovePathStateExplore movePathStateExplore { get; private set; }
    public PlayerIdleStateBattle idleStateBattle { get; private set; }
    public PlayerMovePathStateBattle movePathStateBattle { get; private set; }
    public PlayerComandStateBattle comandStateBattle { get; private set; } 

    [Header("Physic")]
    [SerializeField] private float gravity = 9.8f;
    private float velocity;
    public float xInput { get; private set; }
    public float zInput { get; private set; }

    private void Awake()
    {
        stateMechine = new PlayerStateMachine();

        exploreState = new PlayerExploreState(stateMechine, this);
        readyBattleState = new PlayerReadyBattleState(stateMechine, this);
        battleState = new PlayerBattleState(stateMechine, this);
        waitState = new PlayerWaitState(stateMechine, this);

        idleStateExplore = new PlayerIdleStateExplore(stateMechine, this);
        moveStateExplore = new PlayerMoveStateExplore(stateMechine, this);
        movePathStateExplore = new PlayerMovePathStateExplore(stateMechine, this);

        idleStateBattle = new PlayerIdleStateBattle(stateMechine, this);
        movePathStateBattle = new PlayerMovePathStateBattle(stateMechine, this);
        comandStateBattle = new PlayerComandStateBattle(stateMechine, this);
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        stateMechine.Initialize(exploreState, idleStateExplore);
    }

    private void Update()
    {
        stateMechine.roofState.Update();
        stateMechine.subState.Update();
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

    //  Summary
    //      Move the unit character with the frequence input
    public void Move(float x, float z)
    {
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
        if (world.IsValidWorldRange(targetPosition))
        {
            transform.position = targetPosition;
            return;
        }

        //  Check X axis movement and return
        Vector3 targetPositionX = characterPosition + new Vector3(direction.x, 0, 0) * moveSpeed * Time.deltaTime;
        if (world.IsValidWorldRange(targetPositionX))
        {
            transform.position = targetPositionX;
            return;
        }

        //  Check Z axis movement and return
        Vector3 targetPositionZ = characterPosition + new Vector3(0, 0, direction.z) * moveSpeed * Time.deltaTime;
        if (world.IsValidWorldRange(targetPositionZ))
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

    public void SetMovePosition(PlayerCharacter character, Vector3Int targetPosition)
    {
        List<Vector3> pathVectorList = pathFinding.GetPathRoute(character.transform.position, targetPosition, 1, 1).pathRouteList;
        if (pathVectorList.Count != 0)
        {
            PathRoute pathRoute = new PathRoute
            {
                character = character,
                targetPosition = targetPosition,
                pathRouteList = pathVectorList,
                pathIndex = 0
            };
            character.SetPathRoute(pathRoute);
            stateMechine.ChangeSubState(movePathStateExplore);
        }
    }

    public override void ReadyBattle()
    {
        stateMechine.ChangeRoofState(readyBattleState);
        stateMechine.ChangeSubState(movePathStateBattle);
    }
}