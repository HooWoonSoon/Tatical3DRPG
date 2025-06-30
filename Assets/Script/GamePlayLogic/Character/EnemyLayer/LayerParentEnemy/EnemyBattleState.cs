public class EnemyBattleState : EnemyBaseState
{
    public EnemyBattleState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.stateMechine.ChangeSubState(character.movePathStateBattle);
    }

    public override void Exit()
    {
        base.Exit();
    }
}

