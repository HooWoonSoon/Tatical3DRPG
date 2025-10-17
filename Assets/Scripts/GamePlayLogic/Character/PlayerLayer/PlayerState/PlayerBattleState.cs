using UnityEngine;
using System;

public class PlayerBattleState : PlayerBaseState
{
    public enum BattlePhase
    {
        Ready, Wait, MoveComand, SkillComand, SkillTarget, Move, SkillCast,
        ReleaseMoveComand,
        End
    }

    private BattlePhase currentPhase;
    private Action skillChangedHandler;
    private GameNode confirmMoveNode;
    private bool moveTargetConfirmed = false;
    private bool skillCastConfirmed = false;
    private bool endTurnConfirmed = false;

    private GameNode targetNode;
    private SkillData currentSkill;

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
                    endTurnConfirmed = false;
                }
                break;
            case BattlePhase.MoveComand:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    GameNode selectedNode = BattleManager.instance.GetSelectedGameNode();
                    character.ShowDangerMovableAndTargetTilemap(selectedNode);
                }
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    GameNode moveTargetNode = BattleManager.instance.GetSelectedGameNode();
                    if (!character.IsInMovableRange(moveTargetNode)) return;

                    confirmMoveNode = moveTargetNode;
                    if (confirmMoveNode != character.GetCharacterOriginNode())
                    {
                        moveTargetConfirmed = true;
                    }
                    BattleManager.instance.GeneratePreviewCharacterInMovableRange(character);
                    ChangePhase(BattlePhase.SkillComand);
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    confirmMoveNode = character.GetCharacterOriginNode();
                    endTurnConfirmed = true;
                    ChangePhase(BattlePhase.End);
                }
                break;
            case BattlePhase.SkillComand:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (moveTargetConfirmed)
                    {
                        moveTargetConfirmed = false;
                    }
                    ChangePhase(BattlePhase.MoveComand);
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    ChangePhase(BattlePhase.SkillTarget);
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    endTurnConfirmed = true;
                    ChangePhase(BattlePhase.Move);
                }
                break;
            case BattlePhase.SkillTarget:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    GameNode selectedNode = BattleManager.instance.GetSelectedGameNode();
                    targetNode = character.GetSkillTargetShowTilemap(confirmMoveNode, selectedNode);
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ChangePhase(BattlePhase.SkillComand);
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (targetNode == null) { return; }
                    if (currentSkill.targetType != SkillTargetType.None)
                    {
                        if (targetNode.GetUnitGridCharacter() == null) { return; }
                        ChangePhase(BattlePhase.Move);
                    }
                    else
                    {
                        ChangePhase(BattlePhase.Move);
                    }
                }
                break;
            case BattlePhase.ReleaseMoveComand:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    GameNode selectedNode = BattleManager.instance.GetSelectedGameNode();
                    character.ShowDangerMovableAndTargetTilemap(selectedNode);
                }
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    GameNode moveTargetNode = BattleManager.instance.GetSelectedGameNode();
                    if (!character.IsInMovableRange(moveTargetNode)) return;

                    confirmMoveNode = moveTargetNode;
                    if (confirmMoveNode != character.GetCharacterOriginNode())
                    {
                        moveTargetConfirmed = true;
                    }
                    BattleManager.instance.GeneratePreviewCharacterInMovableRange(character);
                    if (moveTargetConfirmed)
                        ChangePhase(BattlePhase.Move);
                    else
                        ChangePhase(BattlePhase.End);
                }
                break;
            case BattlePhase.Move:
                character.PathToTarget();
                if (!skillCastConfirmed && !endTurnConfirmed)
                {
                    if (character.pathRoute == null)
                    {
                        ChangePhase(BattlePhase.SkillCast);
                    }
                }
                else
                { 
                    if (character.pathRoute == null)
                    {
                        ChangePhase(BattlePhase.End);
                    }
                }
                break;
            case BattlePhase.SkillCast:
                if (moveTargetConfirmed)
                {
                    endTurnConfirmed = true;
                    ChangePhase(BattlePhase.End);
                }
                else
                {
                    skillCastConfirmed = true;
                    ChangePhase(BattlePhase.ReleaseMoveComand);
                }
                break;
            case BattlePhase.End:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    BattleManager.instance.HideOrientationArrow();
                    BattleManager.instance.OnLoadNextTurn();
                    ChangePhase(BattlePhase.Wait);
                }
                break;
        }
    }

    public void ChangePhase(BattlePhase newPhase)
    {
        ExitPhase(currentPhase);
        currentPhase = newPhase;
        EnterPhase(newPhase);
        Debug.Log($"Enter: {currentPhase}");
    }

    public void ExitPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.SkillComand:
                SkillUIManager.instance.onListOptionChanged -= skillChangedHandler;
                break;
        }
    }

    private void EnterPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Ready:
                break;
            case BattlePhase.Wait:
                break;
            case BattlePhase.MoveComand:
                character.ShowDangerAndMovableTileFromNode();
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                BattleManager.instance.SetGridCursorAt(character.GetCharacterOriginNode());

                //  If get return to move command phase
                BattleManager.instance.DestoryPreviewModel();
                BattleUIManager.instance.ActivateActionPanel(true);
                BattleUIManager.instance.CloseSkillUI();
                break;
            case BattlePhase.SkillComand:
                skillChangedHandler = () =>
                {
                    currentSkill = SkillUIManager.instance.GetCurrentSelectedSkill();
                    character.SetSkill(currentSkill);
                    character.ShowSkillTilemap(confirmMoveNode);
                };

                SkillUIManager.instance.onListOptionChanged += skillChangedHandler;
                BattleManager.instance.SetGridCursorAt(confirmMoveNode);
                BattleManager.instance.ActivateMoveCursorAndHide(false, false);
                BattleUIManager.instance.ActivateActionPanel(false);
                BattleUIManager.instance.OpenUpdateSkillUI(character);

                currentSkill = SkillUIManager.instance.GetCurrentSelectedSkill();
                character.ShowSkillTilemap(confirmMoveNode);
                break;
            case BattlePhase.SkillTarget:
                BattleManager.instance.ActivateMoveCursorAndHide(true, false);
                BattleUIManager.instance.CloseSkillUI();
                break;
            case BattlePhase.ReleaseMoveComand:
                character.ShowDangerAndMovableTileFromNode();
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                BattleManager.instance.SetGridCursorAt(character.GetCharacterOriginNode());
                break;
            case BattlePhase.Move:
                BattleManager.instance.ActivateMoveCursorAndHide(false, true);
                BattleUIManager.instance.CloseSkillUI();
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                character.SetPathRoute(confirmMoveNode);
                character.ShowDangerMovableAndTargetTilemap(confirmMoveNode);
                break;
            case BattlePhase.SkillCast:
                BattleManager.instance.DestoryPreviewModel();
                break;
            case BattlePhase.End:
                character.ResetVisualTilemap();
                BattleManager.instance.ActivateMoveCursorAndHide(false, true);
                BattleManager.instance.SetupOrientationArrow(character, confirmMoveNode);
                BattleManager.instance.DestoryPreviewModel();
                break;
        }
    }
}
