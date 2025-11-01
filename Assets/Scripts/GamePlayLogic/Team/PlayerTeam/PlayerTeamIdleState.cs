public class PlayerTeamIdleState : PlayerTeamState
{
    public PlayerTeamIdleState(TeamStateMachine stateMachine, PlayerTeamSystem team) : base(stateMachine, team)
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
        team.currentLeader.MovementInput(out float inputX, out float inputZ);
        if (inputX != 0 || inputZ != 0)
        {
            team.stateMachine.ChangeState(team.teamActionState);
        }
    }
}

