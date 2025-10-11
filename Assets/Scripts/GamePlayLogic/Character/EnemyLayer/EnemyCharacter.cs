using UnityEngine.TextCore.Text;

public class EnemyCharacter : CharacterBase
{
    public EnemyStateMachine stateMechine;

    public EnemyExploreState exploreState { get; private set; }
    public EnemyIdleStateExplore idleStateExplore { get; private set; }
    public EnemyBattleState battleState { get; private set; }

    private void Awake()
    {
        stateMechine = new EnemyStateMachine();

        exploreState = new EnemyExploreState(stateMechine, this);
        idleStateExplore = new EnemyIdleStateExplore(stateMechine, this);
        battleState = new EnemyBattleState(stateMechine, this);
    }
    protected override void Start()
    {
        base.Start();
        stateMechine.Initialize(exploreState);
    }
    private void Update()
    {
        stateMechine.currentState.Update();
    }
    public override void ReadyBattle()
    {
        stateMechine.ChangeState(battleState);
    }
}

