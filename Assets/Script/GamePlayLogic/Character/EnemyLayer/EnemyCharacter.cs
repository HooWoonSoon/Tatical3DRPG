public class EnemyCharacter : CharacterBase
{
    public EnemyStateMachine stateMechine;

    public EnemyExploreState exploreState;
    public EnemyBattleState battleState;
    public EnemyIdleStateExplore idleStateExplore { get; private set; }
    public EnemyMovePathStateBattle movePathStateBattle { get; private set; }

    private void Awake()
    {
        stateMechine = new EnemyStateMachine();

        exploreState = new EnemyExploreState(stateMechine, this);
        battleState = new EnemyBattleState(stateMechine, this);

        idleStateExplore = new EnemyIdleStateExplore(stateMechine, this);
        movePathStateBattle = new EnemyMovePathStateBattle(stateMechine, this);
    }
    protected override void Start()
    {
        base.Start();
        stateMechine.Initialize(exploreState, idleStateExplore);
    }

    private void Update()
    {
        stateMechine.subState.Update();
    }

    public override void EnterBattle()
    {
        stateMechine.ChangeRoofState(battleState);
    }
}

