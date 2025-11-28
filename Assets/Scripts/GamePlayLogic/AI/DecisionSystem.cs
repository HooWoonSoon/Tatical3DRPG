using System.Collections.Generic;
using UnityEngine;
public class DecisionSystem
{
    private World world;

    public CharacterBase decisionMaker;

    private SkillData skill;
    private GameNode moveToNode;
    private GameNode skillTargetNode;

    private float shootOffsetHeight = 1.5f;
    
    public DecisionSystem(World world, CharacterBase character)
    {
        this.world = world;
        decisionMaker = character;
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

        foreach (SkillData skill in decisionMaker.skillDatas)
        {
            List<GameNode> skillInflueneMovableNode = decisionMaker.GetSkillAttackMovableNode(skill);

            //Debug.Log($"Skill Node: {skillInflueneMovableNode.Count}");
            if (skillInflueneMovableNode.Count == 0) continue;

            float bestSkillScore = 0;
            GameNode bestSkillInflueneNode = null;

            foreach (GameNode node in skillInflueneMovableNode)
            {
                float score = 0;
                List<CharacterBase> influenceCharacter = decisionMaker.GetSkillAttackableCharacter(skill, node);
                Debug.Log($"Influence Character Count: {influenceCharacter.Count}");

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
                    if (skill.damageAmount >= target.currentHealth)
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

        List<GameNode> optionNode = decisionMaker.GetSkillRangeFromNode(skill, originNode);

        foreach (GameNode node in optionNode)
        {
            float score = 0;
            CharacterBase targetCharacter = node.GetUnitGridCharacter();

            if (targetCharacter == decisionMaker) { continue; }
            if (targetCharacter != null)
            {
                if (skill.isProjectile)
                {
                    Parabola parabola = new Parabola(world);
                    UnitDetectable unit = parabola.GetParabolaHitUnit
                        (originNode.GetVector() + new Vector3(0, shootOffsetHeight, 0), node.GetVector(), skill.initialElevationAngle);

                    if (unit == null) { continue; }
                    CharacterBase hitCharacter = unit.GetComponent<CharacterBase>();
                    if (hitCharacter != targetCharacter) { continue; }
                }

                if (skill.damageAmount >= targetCharacter.currentHealth)
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
