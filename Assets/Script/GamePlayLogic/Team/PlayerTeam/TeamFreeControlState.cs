public class TeamFreeControlState : PlayerTeamState
{
    public TeamFreeControlState(TeamStateMachine stateMachine, PlayerTeamSystem team) : base(stateMachine, team)
    {
    }

    public override void Enter()
    {
        base.Enter();
        for (int i = 0; i < team.linkMembers.Count; i++)
        {
            CharacterBase character = team.linkMembers[i].character;
            character.stateMachine.ChangeSubState(character.idleStateExplore);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}

