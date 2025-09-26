using System.Collections.Generic;
using UnityEngine;
public enum UnitType
{
    Melee, Ranged
}
public class DecisionMaker
{
    public CharacterBase character;

    public SkillData skill;
    public GameNode moveToNode;
    
    public DecisionMaker(CharacterBase character)
    {
        this.character = character;
    }

    public void MakeDecision()
    {
        Dictionary<GameNode, float> nodeScore = new Dictionary<GameNode, float>();
        //List<GameNode> movableNode = character.GetMovableNode();
        //List<GameNode> conflictNode = character.GetConflictNode();

        SkillData bestSkill = null;
        GameNode bestNode = null;
        float bestScore = int.MinValue;

        foreach (SkillData skill in character.skillData)
        {
            List<GameNode> skillInflueneNode = character.GetSkillAttackableNode(skill);
            Debug.Log($"Skill Node: {skillInflueneNode.Count}");
            if (skillInflueneNode.Count == 0) continue;
            float score = 0;
            foreach (GameNode node in skillInflueneNode)
            {
                List<CharacterBase> influenceCharacter = character.GetSkillAttackableCharacter(skill, node);
                foreach (CharacterBase target in influenceCharacter)
                {
                    if (skill.baseDamage > target.currenthealth)
                    {
                        score += 50;
                        break;
                    }
                }
                if (influenceCharacter.Count == 1)
                {
                    score += 10;
                }
                else if (influenceCharacter.Count > 1)
                {
                    score += 10 / influenceCharacter.Count;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    bestSkill = skill;
                    bestNode = node;
                }
            }
        }

        if (bestSkill != null && bestNode != null)
        {
            skill = bestSkill;
            moveToNode = bestNode;
        }
    }    

    public void GetResult(out SkillData skill, out GameNode targetNode)
    {
        skill = this.skill;
        targetNode = moveToNode;
    }
}
