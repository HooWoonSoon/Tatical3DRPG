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

        CalculateVelocity();

        transform.Translate(velocity, Space.World);
    }

    private void CalculateVelocity()
    {
        Move(xInput, zInput);
        StepClimb(xInput, zInput, 1f);
        velocity += Vector3.up * gravity * Time.deltaTime;

        if (velocity.z > 0 && CheckForward() || velocity.z < 0 && CheckBackward())
            velocity.z = 0;
        if (velocity.x > 0 && CheckRight() || velocity.x < 0 && CheckLeft())
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckGrounded(velocity.y);
        else if (velocity.y > 0 && CheckUp())
            velocity.y = CheckGrounded(velocity.y);
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
        }

        Vector3 direction = new Vector3(x, 0, z).normalized;
        FacingDirection(direction);
        SetOrientation(direction);
        isMoving = true;

        //  Check movement and return
        
        velocity.x = direction.x * moveSpeed * Time.deltaTime;
        velocity.z = direction.z * moveSpeed * Time.deltaTime;
        Vector3 targetPosition = transform.position + velocity;

        if (!world.IsValidWorldRange(targetPosition))
        {
            velocity.x = 0;
            velocity.z = 0;
        }
    }

    private void StepClimb(float x, float z, float height)
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        Vector3 direction = new Vector3(x, 0, z).normalized;

        if (direction == Vector3.zero) { return; }
        
        float checkUp = centerPos.y - half.y;
        float checkForward = centerPos.z + half.z;
        float checkBackward = centerPos.z - half.z;
        float checkRight = centerPos.x + half.x;
        float checkLeft = centerPos.x - half.x;

        if (CheckForward())
        {
            if (!CheckUp())
            {
                if (world.CheckSolidNode(centerPos.x, checkUp, checkForward))
                {
                    transform.position += new Vector3(0, height, 0);
                }
            }
        }
        else if (CheckLeft())
        {
            if (!CheckUp())
            {
                if (world.CheckSolidNode(checkLeft, checkUp, centerPos.z))
                {
                    transform.position += new Vector3(0, height, 0);
                }
            }
        }
        else if (CheckRight())
        {
            if (!CheckUp())
            {
                if (world.CheckSolidNode(checkRight, checkUp, centerPos.z))
                {
                    transform.position += new Vector3(0, height, 0);
                }
            }
        }
        else if (CheckBackward())
        {
            if (!CheckUp())
            {
                if (world.CheckSolidNode(centerPos.x, checkUp, checkBackward))
                {
                    transform.position += new Vector3(0, height, 0);
                }
            }
        }
    }

    #region Check Collision
    private float CheckGrounded(float downSpeed)
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;

        Vector3 min = centerPos - half;
        Vector3 max = centerPos + half;

        float checkY = min.y + downSpeed;

        if (world.CheckSolidNode(min.x, checkY, min.z) ||
            world.CheckSolidNode(max.x, checkY, min.z) ||
            world.CheckSolidNode(min.x, checkY, max.z) ||
            world.CheckSolidNode(max.x, checkY, max.z))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    public bool CheckForward()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkForward = centerPos.z + half.z;

        if (world.CheckSolidNode(transform.position.x, transform.position.y, checkForward))
            return true;
        else
            return false;
    }

    public bool CheckBackward()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkBackward = centerPos.z - half.z;

        if (world.CheckSolidNode(transform.position.x, transform.position.y, checkBackward))
            return true;
        else
            return false;
    }

    public bool CheckRight()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkRight = centerPos.x + half.x;

        if (world.CheckSolidNode(checkRight, transform.position.y, transform.position.z))
            return true;
        else
            return false;
    }

    public bool CheckLeft()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkLeft = centerPos.x - half.x;

        if (world.CheckSolidNode(checkLeft, transform.position.y, transform.position.z))
            return true;
        else
            return false;
    }

    private bool CheckUp()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkUp = centerPos.y + half.y;

        if (world.CheckSolidNode(transform.position.x, checkUp, transform.position.z))
            return true;
        else
            return false;
    }
    #endregion

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

    private void OnDrawGizmos()
    {
        if (unitDetectable == null) return;

        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;

        Vector3 forwardPos = new Vector3(transform.position.x, transform.position.y, centerPos.z + half.z);
        Vector3 backwardPos = new Vector3(transform.position.x, transform.position.y, centerPos.z - half.z);
        Vector3 rightPos = new Vector3(centerPos.x + half.x, transform.position.y, transform.position.z);
        Vector3 leftPos = new Vector3(centerPos.x - half.x, transform.position.y, transform.position.z);
        Vector3 upPos = new Vector3(transform.position.x, centerPos.y + half.y, transform.position.z);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(forwardPos, 0.1f);
        Gizmos.DrawSphere(backwardPos, 0.1f);
        Gizmos.DrawSphere(rightPos, 0.1f); 
        Gizmos.DrawSphere(leftPos, 0.1f); 
        Gizmos.DrawSphere(upPos, 0.1f);
    }
}