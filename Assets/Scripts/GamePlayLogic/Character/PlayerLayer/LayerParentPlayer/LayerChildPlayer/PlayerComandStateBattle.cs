using UnityEngine;

public class PlayerComandStateBattle : PlayerBaseState
{
    public PlayerComandStateBattle(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.ResetVisualTilemap();
        character.ShowSkillTilemap();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BattleManager.instance.DestoryPreviewModel();
            BattleManager.instance.ActivateMoveCursor(true);
            BattleUIManager.instance.ActivateActionPanel(true);
            BattleUIManager.instance.CloseSkillUI();
            stateMachine.ChangeSubState(character.idleStateBattle);
        }
    }
}
