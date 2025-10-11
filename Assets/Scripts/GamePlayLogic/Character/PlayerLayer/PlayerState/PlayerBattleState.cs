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
                    ChangePhase(BattlePhase.Move);
                }
                break;
            case BattlePhase.SkillTarget:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    GameNode selectedNode = BattleManager.instance.GetSelectedGameNode();
                    character.ShowSkillTargetTilemap(confirmMoveNode, selectedNode);
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ChangePhase(BattlePhase.SkillComand);
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    ChangePhase(BattlePhase.Move);
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
                if (!skillCastConfirmed)
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
                    ChangePhase(BattlePhase.End);
                }
                skillCastConfirmed = true;
                ChangePhase(BattlePhase.ReleaseMoveComand);
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
        currentPhase = newPhase;
        EnterPhase(newPhase);
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
                BattleManager.instance.SetBattleCursorAt(character.GetCharacterOriginNode());

                //  If get return to move command phase
                BattleManager.instance.DestoryPreviewModel();
                BattleUIManager.instance.ActivateActionPanel(true);
                BattleUIManager.instance.CloseSkillUI();
                SkillUIManager.instance.onListOptionChanged -= skillChangedHandler;
                break;
            case BattlePhase.SkillComand:
                skillChangedHandler = () =>
                {
                    character.SetSkill(SkillUIManager.instance.GetCurrentSelectedSkill());
                    character.ShowSkillTilemap(confirmMoveNode);
                };

                SkillUIManager.instance.onListOptionChanged += skillChangedHandler;
                BattleManager.instance.SetBattleCursorAt(confirmMoveNode);
                BattleManager.instance.ActivateMoveCursorAndHide(false, false);
                BattleUIManager.instance.ActivateActionPanel(false);
                BattleUIManager.instance.OpenUpdateSkillUI(character);
                character.ShowSkillTilemap(confirmMoveNode);
                break;
            case BattlePhase.SkillTarget:
                SkillUIManager.instance.onListOptionChanged -= skillChangedHandler;
                BattleManager.instance.ActivateMoveCursorAndHide(true, false);
                BattleUIManager.instance.CloseSkillUI();
                break;
            case BattlePhase.ReleaseMoveComand:
                character.ShowDangerAndMovableTileFromNode();
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                BattleManager.instance.SetBattleCursorAt(character.GetCharacterOriginNode());
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
