using NUnit.Framework;

public class PlayerTeamActionState : PlayerTeamState
{
    public PlayerTeamActionState(TeamStateMachine stateMachine, PlayerTeamSystem team) : base(stateMachine, team)
    {
    }

    public override void Enter()
    {
        base.Enter();
        PlayerTeamLinkUIManager.instance.PopInTeamLinkOptionContent();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        team.currentLeader.MovementInput(out float inputX, out float inputZ);
        team.currentLeader.SetMoveDirection(inputX, inputZ);

        bool anyMoving = false;

        for (int i = 0; i < team.linkMembers.Count; i++)
        {
            var character = team.linkMembers[i].character;
            team.FollowWithNearIndexMember(character, team.linkMembers[i].targetToFollow);

            if (character.xInput != 0 || character.zInput != 0)
            {
                character.UpdateHistory();
                anyMoving = true;
            }
        }

        if (inputX != 0 || inputZ != 0)
        {
            anyMoving = true;
        }

        if (!anyMoving)
        {
            team.stateMachine.ChangeState(team.teamIdleState);
        }
    }
}
