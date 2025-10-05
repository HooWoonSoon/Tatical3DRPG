using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleState : PlayerBaseState
{
    public PlayerBattleState(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.ResetVisualTilemap();
        character.ShowDangerAndMovableTileFromNode();
        CameraMovement.instance.ChangeFollowTarget(character.transform);
        BattleManager.instance.SetBattleCursorAt(character.GetCharacterOriginNode());
        BattleUIManager.instance.ActivateActionPanel(true);
    }

    public override void Exit()
    {
        base.Exit();
        BattleUIManager.instance.CloseSkillUI();
    }

    public override void Update()
    {
        base.Update();
        if (!character.IsYourTurn(character))
        {
            stateMachine.ChangeRoofState(character.waitState);
        }
        if (BattleManager.instance.IsSelectedNodeChange())
        {
            character.ResetVisualTilemap();
            character.ShowDangerMovableAndTargetTilemap(BattleManager.instance.GetSelectedGameNode());
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            BattleManager.instance.GeneratePreviewCharacterInMovableRange(character);
            BattleManager.instance.ActivateMoveCursor(false);
            BattleUIManager.instance.ActivateActionPanel(false);
            BattleUIManager.instance.OpenUpdateSkillUI(character);
            stateMachine.ChangeSubState(character.comandStateBattle);
        }
    }
}
