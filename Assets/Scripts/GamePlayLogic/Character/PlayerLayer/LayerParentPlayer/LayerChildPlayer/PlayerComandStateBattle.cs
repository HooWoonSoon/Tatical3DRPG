
public class PlayerComandStateBattle : PlayerBaseState
{
    public PlayerComandStateBattle(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        BattleUIManager.instance.CloseSkillUI();
        BattleManager.instance.SetBattleCursorAt(character.GetCharacterOriginNode());
        character.ShowSkillTilemap();
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
