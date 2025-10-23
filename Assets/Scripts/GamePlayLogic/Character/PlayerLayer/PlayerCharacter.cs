using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;
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
    #endregion

    public Vector3? targetPosition = null;

    public PlayerStateMachine stateMechine;
    private Animator anim;
    private UnitDetectable unitDetectable;

    public PlayerExploreState exploreState { get; private set; }
    public PlayerDeploymentState deploymentState { get; private set; }
    public PlayerBattleState battleState { get; private set; }

    public PlayerIdleStateExplore idleStateExplore { get; private set; }
    public PlayerMoveStateExplore moveStateExplore { get; private set; }
    public PlayerMovePathStateExplore movePathStateExplore { get; private set; }

    [Header("Physic")]
    [SerializeField] private float gravity = -9.8f;
    private bool isGrounded;
    private Vector3 velocity;
    public float xInput { get; private set; }
    public float zInput { get; private set; }

    private void Awake()
    {
        stateMechine = new PlayerStateMachine();

        exploreState = new PlayerExploreState(stateMechine, this);
        deploymentState = new PlayerDeploymentState(stateMechine, this);
        battleState = new PlayerBattleState(stateMechine, this);

        idleStateExplore = new PlayerIdleStateExplore(stateMechine, this);
        moveStateExplore = new PlayerMoveStateExplore(stateMechine, this);
        movePathStateExplore = new PlayerMovePathStateExplore(stateMechine, this);
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        stateMechine.Initialize(exploreState);
        unitDetectable = GetComponent<UnitDetectable>();

        MapDeploymentManager.instance.onStartDeployment += () =>
        {
            stateMechine.ChangeState(deploymentState);
        };
    }

    private void Update()
    {
        stateMechine.currentState.Update();

        velocity += Vector3.up * gravity * Time.deltaTime;
        velocity.y = CheckGrounded(velocity.y);
        Move(xInput, zInput);

        transform.Translate(velocity, Space.World);
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
    //public void TeleportToPos(Vector3 position)
    //{
    //    GameNode targetNode = world.GetNode(position);
    //    if (targetNode != null)
    //    {
    //        transform.position = position;
    //        targetNode.SetUnitGridCharacter(this);
    //        stateMechine.ChangeState(idleStateExplore);
    //    }
    //}

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
        SetOrientation(direction);

        //  Check movement and return
        velocity = direction * moveSpeed * Time.deltaTime;
        Vector3 targetPosition = characterPosition + velocity;

        if (!world.IsValidWorldRange(targetPosition))
        {
            velocity = Vector3.zero;
        }
    }
    
    private float CheckGrounded(float gravity)
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;

        Vector3 min = centerPos - half;
        Vector3 max = centerPos + half;

        float checkY = min.y + gravity; 
        int minX = Mathf.FloorToInt(min.x);
        int maxX = Mathf.FloorToInt(max.x);
        int minZ = Mathf.FloorToInt(min.z);
        int maxZ = Mathf.FloorToInt(max.z);

        if (world.CheckSolidNode(minX, checkY, minZ) ||
            world.CheckSolidNode(maxX, checkY, minZ) ||
            world.CheckSolidNode(minX, checkY, maxZ) ||
            world.CheckSolidNode(maxX, checkY, maxZ))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return gravity;
        }
    }

    public void SetMovePosition(Vector3 targetPosition)
    {
        Vector3Int targetPos = Utils.RoundXZFloorYInt(targetPosition);
        SetMovePosition(targetPos);
    }

    public void SetMovePosition(Vector3Int targetPosition)
    {
        List<Vector3> pathVectorList = pathFinding.GetPathRoute(transform.position, targetPosition, 1, 1).pathRouteList;
        if (pathVectorList.Count != 0)
        {
            PathRoute pathRoute = new PathRoute
            {
                character = this,
                targetPosition = targetPosition,
                pathRouteList = pathVectorList,
                pathIndex = 0
            };
            SetPathRoute(pathRoute);
            stateMechine.ChangeState(movePathStateExplore);
        }
    }

    public IEnumerator MoveToPositionCoroutine(Vector3 targetPosition, Action onArrived = null)
    {
        SetMovePosition(targetPosition);

        while (pathRoute != null && pathRoute.pathIndex < pathRoute.pathRouteList.Count)
        {
            yield return null;
        }

        onArrived?.Invoke();
    }

    public override void TeleportToNode(GameNode targetNode)
    {
        if (targetNode != null)
        {
            SetSelfToNode(targetNode, 0.5f);
            stateMechine.ChangeState(idleStateExplore);
        }
    }

    public override void ReadyBattle()
    {
        stateMechine.ChangeState(battleState);
    }
}