using System.Collections.Generic;
using UnityEngine;
public enum UnitType
{
    Melee, Ranged
}
public class DecisionMaker
{
    public CharacterBase character;

    private SkillData skill;
    private GameNode moveToNode;
    private GameNode skillTargetNode;
    
    public DecisionMaker(CharacterBase character)
    {
        this.character = character;
        MakeDecision();
    }

    public void MakeDecision()
    {
        //Dictionary<GameNode, float> nodeScore = new Dictionary<GameNode, float>();
        //List<GameNode> movableNode = character.GetMovableNode();
        //List<GameNode> conflictNode = character.GetConflictNode();

        SkillData bestSkill = null;
        GameNode bestMoveNode = null;
        GameNode bestSkillTargetNode = null;

        float bestScore = int.MinValue;

        foreach (SkillData skill in character.skillData)
        {
            List<GameNode> skillInflueneMovableNode = character.GetSkillAttackMovableNode(skill);

            Debug.Log($"Skill Node: {skillInflueneMovableNode.Count}");
            if (skillInflueneMovableNode.Count == 0) continue;

            float bestSkillScore = 0;
            GameNode bestSkillInflueneNode = null;

            foreach (GameNode node in skillInflueneMovableNode)
            {
                float score = 0;
                List<CharacterBase> influenceCharacter = character.GetSkillAttackableCharacter(skill, node);

                if (influenceCharacter.Count == 1)
                {
                    score += 10;
                }
                else if (influenceCharacter.Count > 1)
                {
                    score += 10 / influenceCharacter.Count;
                }
                foreach (CharacterBase target in influenceCharacter)
                {
                    if (skill.power >= target.currenthealth)
                    {
                        score += 50;
                        break;
                    }
                }
                if (score >= bestSkillScore)
                {
                    bestSkillScore = score;
                    bestSkillInflueneNode = node;
                }
            }
            
            if (bestSkillScore >= bestScore)
            {
                bestScore = bestSkillScore;
                bestMoveNode = bestSkillInflueneNode;
                bestSkill = skill;
                bestSkillTargetNode = GetSkillCastNode(bestSkill, bestMoveNode);
            }
        }

        if (bestMoveNode != null)
        {
            skill = bestSkill;
            moveToNode = bestMoveNode;
            skillTargetNode = bestSkillTargetNode;
        }
    }    

    public GameNode GetSkillCastNode(SkillData skill, GameNode originNode)
    {
        float bestScore = int.MinValue;
        GameNode bestNode = null;

        List<GameNode> optionNode = character.GetSkillRangeFromNode(skill, originNode);
        foreach (GameNode node in optionNode)
        {
            float score = 0;
            CharacterBase character = node.GetUnitGridCharacter();
            if (character != null)
            {
                if (skill.power >= character.currenthealth)
                {
                    score += 50;
                }
                else
                {
                    score += 10;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestNode = node;
                }
            }
        }
        return bestNode;
    }

    public void GetResult(out SkillData skill, out GameNode moveToNode, out GameNode skillTargetNode)
    {
        skill = this.skill;
        moveToNode = this.moveToNode;
        skillTargetNode = this.skillTargetNode;
    }

    public GameNode GetMoveNode => moveToNode;
    public SkillData GetSkill => skill;
    public GameNode GetSkillTargetNode => skillTargetNode;
}
