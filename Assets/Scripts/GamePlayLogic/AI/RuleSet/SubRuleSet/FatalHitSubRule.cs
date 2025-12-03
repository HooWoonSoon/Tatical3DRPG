using UnityEngine;
public class FatalHitSubRule : ScoreSubRuleBase
{
    public FatalHitSubRule(int scoreBonus, bool debugMode) : base(scoreBonus, debugMode)
    {
    }

    public override float CalculateScore(CharacterBase character, SkillData skill, 
        GameNode moveNode, GameNode targetNode)
    {
        //  No Join the Rule
        if (skill == null) return 0;

        CharacterBase target = targetNode.character;
        if (target == null) return 0;

        if (debugMode)
        {
            Debug.Log("Execute Fatal Hit Rule");
            if (target != null)
                Debug.Log($"{character.data.characterName} consider to use " +
                    $"{skill.skillName} on {target.data.characterName}");
        }

        if (skill.damageAmount >= target.currentHealth)
        {
            if (debugMode)
                Debug.Log($"Skill: {skill.skillName} is fatal skill, " +
                    $"plus Score Bonus : {scoreBonus}");
            return scoreBonus;
        }
        return 0;
    }
}
