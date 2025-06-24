
public class MoveStateExplore : CharacterBaseState
{
    public MoveStateExplore(StateMachine stateMachine, Character character) : base(stateMachine, character)
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
        if (TeamFollowPathFinding.instance.isActivePathFinding) { return; }

        character.Move(character.xInput, character.zInput);
        character.UpDownHill(character.xInput, character.zInput);

        if (character.xInput == 0 && character.zInput == 0)
        {
            stateMachine.ChangeSubState(character.idleStateExplore);
        }
    }
}

