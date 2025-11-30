using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tactics.AI
{
    public class DecisionSystem
    {
        List<IScoreRule> rules = new List<IScoreRule>();
        private World world;
        private PathFinding pathFinding;

        public CharacterBase decisionMaker;

        private SkillData skill;
        private GameNode moveNode;
        private GameNode skillTargetNode;

        private float shootOffsetHeight = 1.5f;

        private bool debugMode = false;

        public DecisionSystem(World world, CharacterBase character, bool debugMode = false)
        {
            this.world = world;
            decisionMaker = character;
            this.debugMode = debugMode;

            PathFinding pathfinding = new PathFinding(world);
            this.pathFinding = pathfinding;

            rules.Add(new PrimaryTargetRule(pathfinding, 20, false));

            //  Move Rule
            rules.Add(new UnitMoveRule(pathfinding, 5, true));
            
            //  SkillRule
            rules.Add(new FatalHitRule(pathfinding, 50, false));
            rules.Add(new HarmRule(pathfinding, 30, false));
            rules.Add(new TreatRule(pathfinding, 30, false));
        }

        public void MakeDecision()
        {
            float moveBestScore = float.MinValue;
            GameNode bestMoveNode = null;
            EvaluateMoveTargetOption(ref moveBestScore, out bestMoveNode);

            float skillBestScore = float.MinValue;
            SkillData bestSkill = null;
            GameNode bestSkillMoveNode = null;
            GameNode bestSkillTargetNode = null;
            EvaluateSkillOption(ref skillBestScore, out bestSkill, 
                out bestSkillMoveNode, out bestSkillTargetNode);

            if (bestSkill != null && skillBestScore > moveBestScore)
            {
                this.skill = bestSkill;
                this.moveNode = bestSkillMoveNode;
                this.skillTargetNode = bestSkillTargetNode;
                if (debugMode)
                    Debug.Log($"Decision: Use Skill {bestSkill.skillName} at {bestSkillTargetNode.GetNodeVectorInt()}");
            }
            else
            {
                this.skill = null;
                this.moveNode = bestMoveNode;
                if (debugMode && bestMoveNode != null)
                    Debug.Log($"Decision: Move to {bestMoveNode.GetNodeVectorInt()}");
            }
        }

        private void EvaluateMoveTargetOption(ref float bestScore, out GameNode bestNode)
        {
            bestNode = null;
            CharacterBase primaryTarget = GetPrimaryTarget();
            if (primaryTarget == null) return;

            List<GameNode> movableNodes = decisionMaker.GetMovableNode();

            foreach (var moveNode in movableNodes)
            {
                float totalScore = 0;

                foreach (var rule in rules)
                    totalScore += rule.CalculateMoveToTargetScore(decisionMaker, primaryTarget, moveNode);

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestNode = moveNode;
                }
            }
        }
        private void EvaluateSkillOption(ref float bestScore, 
            out SkillData bestSkill, out GameNode bestMoveNode, out GameNode bestSkillTargetNode)
        {
            bestSkill = null;
            bestMoveNode = null;
            bestSkillTargetNode = null;

            foreach (SkillData skill in decisionMaker.skillDatas)
            {
                if (skill.isTargetTypeSkill)
                {
                    List<GameNode> skillTargetableMovableNode = GetSkillAttackMovableNode(decisionMaker, skill);

                    if (debugMode)
                    {
                        List<GameNode> movableNodes = decisionMaker.GetMovableNode();
                        string log1 = string.Join(",",
                            movableNodes.ConvertAll(m => m.GetNodeVector().ToString()));
                        string log2 = string.Join(",",
                            skillTargetableMovableNode.ConvertAll(s => s.GetNodeVector().ToString()));
                        Debug.Log(
                            $"Character: {decisionMaker.data.characterName}, " +
                            $"Origin Node: {decisionMaker.currentNode.GetNodeVector()}, " +
                            $"Movable Node: {log1}, " +
                            $"Skill: {skill.skillName}, " +
                            $"Skill Targetable Movable Node: {log2}");
                    }

                    foreach (var moveNode in skillTargetableMovableNode)
                    {
                        var skilltargetNodes = decisionMaker.GetSkillRangeFromNode(skill, moveNode);
                        foreach (var skillTargetNode in skilltargetNodes)
                        {
                            float totalScore = 0;

                            if (!IsProjectileAchievableSingle(skill, moveNode, skillTargetNode))
                                continue;

                            if (!IsValidSkillTargetNodeSingle(skill, skillTargetNode))
                                continue;
                            else
                                totalScore += 1; // Basic valid target score

                            foreach (var rule in rules)
                                totalScore += rule.CalculateSkillScore(decisionMaker, skill, moveNode, skillTargetNode);

                            if (totalScore > bestScore)
                            {
                                bestScore = totalScore;
                                bestSkill = skill;
                                bestMoveNode = moveNode;
                                bestSkillTargetNode = skillTargetNode;
                            }
                        }
                    }
                }
            }
        }
        private CharacterBase GetPrimaryTarget()
        {
            float bestScore = float.MinValue;
            List<CharacterBase> mapCharacters = decisionMaker.GetMapCharacterExceptSelf();
            CharacterBase primaryTarget = null;

            foreach (CharacterBase character in mapCharacters)
            {
                float totalScore = 0;

                foreach (var rule in rules)
                    totalScore += rule.CalculateTargetScore(character);
                
                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    primaryTarget = character;
                }
            }
            return primaryTarget;
        }

        #region Skill Target Validation
        private bool IsValidSkillTargetNodeSingle(SkillData skill, GameNode targetNode)
        {
            TeamType selfTeam = decisionMaker.data.type;
            CharacterBase targetNodeCharacter = targetNode.character;

            if (targetNodeCharacter == null) return false;

            UnitState unitState = targetNodeCharacter.unitState;            
            if (unitState == UnitState.Knockout || unitState == UnitState.Dead) 
            {
                if (debugMode)
                    Debug.Log("Skill target character is knockout or dead");
                return false; 
            }

            switch (skill.skillTargetType)
            {
                case SkillTargetType.Opposite:
                    if (targetNodeCharacter.data.type != selfTeam)
                        return true;
                    break;

                case SkillTargetType.Our:
                    if (targetNodeCharacter.data.type == selfTeam)
                        return true;
                    break;

                case SkillTargetType.Self:
                    if (targetNodeCharacter == decisionMaker)
                        return true;
                    break;

                case SkillTargetType.Both:
                    return true;
            }
            return false;
        }
        private bool IsProjectileAchievableSingle(SkillData skill, GameNode startNode,
            GameNode targetNode)
        {
            if (skill.isProjectile)
            {
                Parabola parabola = new Parabola(world);
                if (targetNode.character != null)
                {
                    UnitDetectable unit = parabola.GetParabolaHitUnit
                        (startNode.GetNodeVector() + new Vector3(0, shootOffsetHeight, 0),
                        targetNode.GetNodeVector(),
                        skill.initialElevationAngle);

                    if (unit != null)
                    {
                        CharacterBase hitCharacter = unit.GetComponent<CharacterBase>();
                        CharacterBase targetCharacter = targetNode.character;
                        if (hitCharacter == targetCharacter) return true;
                    }
                }
            }
            else
            {
                if (debugMode)
                    Debug.LogWarning("Called on non-projectile skill");
                return true;
            }
            return false;
        }
        #endregion
        private List<GameNode> GetSkillAttackMovableNode(CharacterBase character, SkillData skill)
        {
            List<GameNode> movableNodes = character.GetMovableNode();
            HashSet<GameNode> result = new HashSet<GameNode>();

            if (!skill.isTargetTypeSkill) return movableNodes;

            GameNode originNode = character.currentNode;

            foreach (GameNode moveNode in movableNodes)
            {
                CharacterBase originNodeCharacter = originNode.character;
                CharacterBase moveNodeCharacter = moveNode.character;

                //  Simulate character change occupyied node
                if (moveNode != originNode)
                {
                    originNode.character = null;
                    moveNode.character = character;
                }

                List<GameNode> influenceNodes = skill.GetInflueneNode(world, moveNode);

                foreach (GameNode node in influenceNodes)
                {
                    if (!IsValidSkillTargetNodeSingle(skill, node)) continue;
                    
                    if (debugMode)
                    {
                        Debug.Log(
                            $"MovableNode: {moveNode.GetNodeVectorInt()}, " +
                            $"Skill: {skill.skillName}, " +
                            $"Influence Node {node.GetNodeVectorInt()}");
                    }
                    result.Add(moveNode);
                }

                //  Revert character occupyied node
                originNode.character = character;
                moveNode.character = moveNodeCharacter;
            }
            return result.ToList();
        }

        #region External Methods
        public void GetResult(out SkillData skill, out GameNode moveToNode, out GameNode skillTargetNode)
        {
            skill = this.skill;
            moveToNode = this.moveNode;
            skillTargetNode = this.skillTargetNode;
        }
        #endregion

        #region Odd Structure Decision Making
        public void MakeDecisionOddStructure()
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
                List<GameNode> skillTargetableMovableNode = GetSkillAttackMovableNode(decisionMaker, skill);

                //Debug.Log($"Skill Node: {skillInflueneMovableNode.Count}");
                if (skillTargetableMovableNode.Count == 0) continue;

                float bestSkillScore = 0;
                GameNode bestSkillInflueneNode = null;

                foreach (GameNode node in skillTargetableMovableNode)
                {
                    float score = 0;
                    List<CharacterBase> influenceCharacter = GetSkillAttackableCharacter(skill, node);
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
                moveNode = bestMoveNode;
                skillTargetNode = bestSkillTargetNode;
            }
        }
        public List<CharacterBase> GetSkillAttackableCharacter(SkillData skill, GameNode gameNode)
        {
            HashSet<CharacterBase> result = new HashSet<CharacterBase>();
            List<GameNode> influenceNodes = skill.GetInflueneNode(world, gameNode);

            foreach (GameNode node in influenceNodes)
            {
                if (node.character == null) { continue; }
                if (node.character == decisionMaker) { continue; }
                if (node.character.currentTeam == null)
                {
                    result.Add(node.character);
                    continue;
                }

                if (node.character.currentTeam == decisionMaker.currentTeam) continue;

                result.Add(node.character);
            }
            return result.ToList();
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
                            (originNode.GetNodeVector() + new Vector3(0, shootOffsetHeight, 0), node.GetNodeVector(), skill.initialElevationAngle);

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
        #endregion
    }
}
