public class EnemyReadyBattleState : EnemyBaseState
{
    public EnemyReadyBattleState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        character.PathToTarget();
        if (character.pathRoute == null)
        {
            character.stateMechine.ChangeRoofState(character.waitState);
            character.stateMechine.ChangeSubState(character.idleStateBattle);
        }
    }
}
