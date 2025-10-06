using UnityEngine;

public class PlayerBattleState : PlayerBaseState
{
    public enum BattlePhase
    {
        Ready, Wait, MoveComand, SkillComand, Move, End
    }

    private BattlePhase currentPhase;

    public PlayerBattleState(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.ResetVisualTilemap();
        currentPhase = BattlePhase.Ready;

    }

    public override void Exit()
    {
        base.Exit();
        BattleUIManager.instance.CloseSkillUI();
    }

    public override void Update()
    {
        base.Update();
        switch (currentPhase)
        {
            case BattlePhase.Ready:
                character.PathToTarget();
                if (character.pathRoute == null)
                {
                    ChangePhase(BattlePhase.Wait);
                }
                break;
            case BattlePhase.Wait:
                if (character.IsYourTurn(character))
                {
                    ChangePhase(BattlePhase.MoveComand);
                }
                break;
            case BattlePhase.MoveComand:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    character.ShowDangerMovableAndTargetTilemap(BattleManager.instance.GetSelectedGameNode());
                }
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (!character.IsInMovableRange(BattleManager.instance.GetSelectedGameNode())) return;

                    BattleManager.instance.GeneratePreviewCharacterInMovableRange(character);
                    BattleManager.instance.ActivateMoveCursor(false);
                    BattleUIManager.instance.ActivateActionPanel(false);
                    BattleUIManager.instance.OpenUpdateSkillUI(character);
                    ChangePhase(BattlePhase.SkillComand);
                }
                break;
            case BattlePhase.SkillComand:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    BattleManager.instance.DestoryPreviewModel();
                    BattleManager.instance.ActivateMoveCursor(true);
                    BattleUIManager.instance.ActivateActionPanel(true);
                    BattleUIManager.instance.CloseSkillUI();
                    ChangePhase(BattlePhase.MoveComand);
                }
                break;
            case BattlePhase.Move:
                character.PathToTarget();
                if (character.pathRoute == null)
                {
                    ChangePhase(BattlePhase.End);
                }
                break;
        }
    }

    public void ChangePhase(BattlePhase newPhase)
    {
        currentPhase = newPhase;
        EnterPhase(newPhase);
    }

    private void EnterPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Ready:
                break;
            case BattlePhase.MoveComand:
                character.ShowDangerAndMovableTileFromNode();
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                BattleManager.instance.SetBattleCursorAt(character.GetCharacterOriginNode());
                BattleUIManager.instance.ActivateActionPanel(true);
                break;
            case BattlePhase.SkillComand:
                SkillUIManager.instance.onSkillChanged += character.ShowSkillTilemap;
                character.ShowSkillTilemap();
                break;
        }
    }
}
