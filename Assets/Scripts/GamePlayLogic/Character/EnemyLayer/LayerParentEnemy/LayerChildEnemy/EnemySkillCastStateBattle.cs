
public class EnemySkillCastStateBattle : EnemyBaseState
{
    public EnemySkillCastStateBattle(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.ShowSkillTargetTilemap();
    }

    public override void Exit()
    {
        base.Exit();
        character.SkillCalculate();
        character.ResetVisualTilemap();
        CTTimeline.instance.NextCharacter();
    }

    public override void Update()
    {
        base.Update();
        if (timeInState >= character.currentSkill.SkillCastTime)
        {
            stateMachine.ChangeSubState(character.idleStateBattle);
        }
    }
}
