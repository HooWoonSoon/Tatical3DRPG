
public class EnemyWaitState : EnemyBaseState
{
    public EnemyWaitState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
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
        if (character.IsYourTurn(character))
        {
            stateMachine.ChangeRoofState(character.battleState);
        }
    }
}

