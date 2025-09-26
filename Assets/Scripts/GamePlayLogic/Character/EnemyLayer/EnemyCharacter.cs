using UnityEngine.TextCore.Text;

public class EnemyCharacter : CharacterBase
{
    public EnemyStateMachine stateMechine;

    public EnemyExploreState exploreState { get; private set; }
    public EnemyReadyBattleState readyBattleState { get; private set; }
    public EnemyBattleState battleState { get; private set; }
    public EnemyWaitState waitState { get; private set; }

    public EnemyIdleStateExplore idleStateExplore { get; private set; }
    public EnemyIdleStateBattle idleStateBattle { get; private set; }
    public EnemyMovePathStateBattle movePathStateBattle { get; private set; }

    private void Awake()
    {
        stateMechine = new EnemyStateMachine();

        exploreState = new EnemyExploreState(stateMechine, this);
        readyBattleState = new EnemyReadyBattleState(stateMechine, this);
        battleState = new EnemyBattleState(stateMechine, this);
        waitState = new EnemyWaitState(stateMechine, this);

        idleStateExplore = new EnemyIdleStateExplore(stateMechine, this);
        idleStateBattle = new EnemyIdleStateBattle(stateMechine, this);
        movePathStateBattle = new EnemyMovePathStateBattle(stateMechine, this);
    }
    protected override void Start()
    {
        base.Start();
        stateMechine.Initialize(exploreState, idleStateExplore);
    }
    private void Update()
    {
        stateMechine.roofState.Update();
        stateMechine.subState.Update();
    }
    public override void ReadyBattle()
    {
        stateMechine.ChangeRoofState(readyBattleState);
        stateMechine.ChangeSubState(idleStateBattle);
    }
}

