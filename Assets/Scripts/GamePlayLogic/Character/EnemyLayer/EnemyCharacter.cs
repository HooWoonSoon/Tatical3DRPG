using UnityEngine.TextCore.Text;

public class EnemyCharacter : CharacterBase
{
    public EnemyStateMachine stateMechine;

    public EnemyExploreState exploreState { get; private set; }
    public EnemyDeploymentState deploymentState { get; private set; }
    public EnemyBattleState battleState { get; private set; }
    public EnemyIdleStateExplore idleStateExplore { get; private set; }

    private void Awake()
    {
        stateMechine = new EnemyStateMachine();

        exploreState = new EnemyExploreState(stateMechine, this);
        battleState = new EnemyBattleState(stateMechine, this);
        deploymentState = new EnemyDeploymentState(stateMechine, this);

        idleStateExplore = new EnemyIdleStateExplore(stateMechine, this);
    }
    protected override void Start()
    {
        base.Start();
        stateMechine.Initialize(exploreState);

        MapDeploymentManager.instance.onStartDeployment += () =>
        {
            stateMechine.ChangeState(deploymentState);
        };
    }
    private void Update()
    {
        stateMechine.currentState.Update();
    }

    public override void TeleportToNodeDeployble(GameNode targetNode)
    {
        if (targetNode != null)
        {
            SetSelfToNode(targetNode, 0.5f);
            stateMechine.ChangeState(deploymentState);
        }
    }
    public override void TeleportToNodeFree(GameNode targetNode)
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

