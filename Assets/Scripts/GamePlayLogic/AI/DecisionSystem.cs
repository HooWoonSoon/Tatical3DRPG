using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tactics.AI
{
    public class DecisionSystem
    {
        public enum Decision
        {
            OriginCastSkill,
            Move,
            MoveAndCastSkill
        }

        List<IScoreRule> rules = new List<IScoreRule>();
        private World world;
        private PathFinding pathFinding;

        public CharacterBase decisionMaker;

        private SkillData skill;
        private GameNode moveNode;
        private GameNode skillTargetNode;

        private float shootOffsetHeight = 1.5f;

        private bool debugMode = false;

        public DecisionSystem(World world, CharacterBase character, 
            bool debugMode = false)
        {
            this.world = world;
            decisionMaker = character;
            this.debugMode = debugMode;

            PathFinding pathfinding = new PathFinding(world);
            this.pathFinding = pathfinding;

            List<IScoreSubRule> targetSubRules = new List<IScoreSubRule>()
            {
            };
            rules.Add(new AgressiveTargetRule(targetSubRules, pathfinding, 20, false));

            //  Move Rule
            List<IScoreSubRule> moveSubRules = new List<IScoreSubRule>()
            {
            };
            rules.Add(new UnitMoveRule(moveSubRules, pathfinding, 25, false));
            
            //  SkillRule
            List<IScoreSubRule> harmSubRules = new List<IScoreSubRule>() 
            { 
                new FatalHitSubRule(10, false) 
            };
            rules.Add(new HarmRule(harmSubRules, pathfinding, 330, true));

            List<IScoreSubRule> treatSubRules = new List<IScoreSubRule>()
            {
            };
            rules.Add(new TreatRule(treatSubRules, pathfinding, 330, true));
        }

        public void MakeDecision(bool allowMove = true, bool allowSkill = true)
        {
            float skillBestScore = float.MinValue;
            SkillData originSkill = null;
            GameNode originSkillTargetNode = null;

            if (allowSkill)
            {
                EvaluateOriginSkillOption(
                    ref skillBestScore, 
                    ref originSkill,
                    ref originSkillTargetNode);
            }

            float moveAndSkillBestScore = float.MinValue;
            SkillData moveSkill = null;
            GameNode moveSkillMoveNode = null;
            GameNode moveSkillTargetNode = null;

            if (allowMove && allowSkill)
            {
                EvaluateMoveAndSkillOption(
                    ref moveAndSkillBestScore, 
                    ref moveSkill, 
                    ref moveSkillMoveNode, 
                    ref moveSkillTargetNode);
            }

            float moveBestScore = float.MinValue;
            GameNode moveOnlyNode = null;

            if (allowMove)
            {
                EvaluateMoveTargetOption(
                    ref moveBestScore, 
                    ref moveOnlyNode);
                if (!allowSkill)
                    EvaluateMoveOption(
                        ref moveBestScore,
                        ref moveOnlyNode);
            }

            float ORIGIN_SKILL_BONUS = 0f;
            float MOVE_SKILL_BONUS = 0f;
            float MOVE_ONLY_BONUS = 0f;

            float finalOriginSkillScore = skillBestScore + ORIGIN_SKILL_BONUS;
            float finalMoveSkillScore = moveAndSkillBestScore + MOVE_SKILL_BONUS;
            float finalMoveScore = moveBestScore + MOVE_ONLY_BONUS;

            float bestFinalScore = finalOriginSkillScore;
            Decision decision = Decision.OriginCastSkill;

            if (finalMoveSkillScore > bestFinalScore)
            {
                bestFinalScore = finalMoveSkillScore;
                decision = Decision.MoveAndCastSkill;
            }
            if (finalMoveScore > bestFinalScore)
            {
                bestFinalScore = finalMoveScore;
                decision = Decision.Move;
            }

            string executeLog = "";
            if (decision == Decision.OriginCastSkill)
            {
                skill = originSkill;
                moveNode = null;
                skillTargetNode = originSkillTargetNode;

                executeLog = 
                    $"Decision: Use Skill {skill.skillName} " +
                    $"at {skillTargetNode.GetNodeVectorInt()}";

            }
            else if (decision == Decision.MoveAndCastSkill)
            {
                skill = moveSkill;
                moveNode = moveSkillMoveNode;
                skillTargetNode = moveSkillTargetNode;

                executeLog = 
                    $"Decision: Move to {moveNode.GetNodeVectorInt()}, " +
                    $"Use Skill {skill.skillName} at {skillTargetNode.GetNodeVectorInt()}";
            }
            else if (decision == Decision.Move)
            {
                skill = null;
                moveNode = moveOnlyNode;
                skillTargetNode = null;

                executeLog = $"Decision: Move to {moveNode.GetNodeVectorInt()}";
            }
            Debug.Log(
                $"{decisionMaker.data.characterName}, " +
                $"Final decision: {decision}, " +
                $"{executeLog}");
        }

        private void EvaluateOriginSkillOption(ref float bestScore,
            ref SkillData bestSkill, ref GameNode bestSkillTargetNode)
        {
            GameNode originNode = decisionMaker.currentNode;

            bestSkill = null;
            bestSkillTargetNode = null;

            foreach (SkillData skill in decisionMaker.skillDatas)
            {
                if (skill.isTargetTypeSkill)
                {
                    var skilltargetNodes = decisionMaker.GetSkillRangeFromNode(skill, originNode);
                    foreach (var skillTargetNode in skilltargetNodes)
                    {
                        float totalScore = 0;

                        if (!IsProjectileAchievableSingle(skill, originNode, skillTargetNode))
                            continue;

                        if (!IsValidSkillTargetNodeSingle(skill, skillTargetNode))
                            continue;

                        foreach (var rule in rules)
                            totalScore += rule.CalculateSkillScore(decisionMaker, skill, originNode, skillTargetNode);

                        if (totalScore > bestScore)
                        {
                            bestScore = totalScore;
                            bestSkill = skill;
                            bestSkillTargetNode = skillTargetNode;
                        }
                    }
                }
            }
        }
        private void EvaluateMoveAndSkillOption(ref float bestScore, 
            ref SkillData bestSkill, ref GameNode bestMoveNode, 
            ref GameNode bestSkillTargetNode)
        {
            bestSkill = null;
            bestMoveNode = null;
            bestSkillTargetNode = null;

            foreach (SkillData skill in decisionMaker.skillDatas)
            {
                if (skill.isTargetTypeSkill)
                {
                    List<GameNode> skillTargetableMovableNode = GetSkillTargetableMovableNode(decisionMaker, skill);

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
        private void EvaluateMoveTargetOption(ref float bestScore, ref GameNode bestNode)
        {
            CharacterBase primaryTarget = GetPrimaryTarget();
            if (primaryTarget == null) return;
            List<GameNode> targetAroundNodes = GetTargetAroundNodes(primaryTarget);

            List<GameNode> movableNodes = decisionMaker.GetMovableNode();
            foreach (var moveNode in movableNodes)
            {
                float totalScore = 0;

                foreach (var rule in rules)
                    totalScore += rule.CalculateMoveToTargetScore(decisionMaker, targetAroundNodes, moveNode);

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestNode = moveNode;
                }
            }
        }

        private void EvaluateMoveOption(ref float bestScore, 
            ref GameNode bestNode)
        {
            Debug.Log("Execute Evaluate Move Option");
            List<GameNode> movableNodes = decisionMaker.GetMovableNode();
            List<GameNode> conflictNodes = decisionMaker.GetConflictNode();
            List<GameNode> supportNode = decisionMaker.GetSupportNode();

            List<CharacterBase> mapCharactersExceptSelf = 
                decisionMaker.GetMapCharacterExceptSelf();
            List<CharacterBase> opposites = 
                GetOppositeCharacter(decisionMaker, mapCharactersExceptSelf);
            List<CharacterBase> teammates =
                GetSameTeamCharacter(decisionMaker, mapCharactersExceptSelf);

            foreach (var moveNode in movableNodes)
            {
                float totalScore = 0;

                foreach (var rule in rules)
                {
                    totalScore += rule.CalculateMoveScore(decisionMaker,
                        conflictNodes, opposites, supportNode, teammates, moveNode);
                }

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestNode = moveNode;
                }
            }
        }

        private CharacterBase GetPrimaryTarget()
        {
            float bestScore = float.MinValue;
            List<CharacterBase> mapCharactersExceptSelf = decisionMaker.GetMapCharacterExceptSelf();
            CharacterBase primaryTarget = null;

            foreach (CharacterBase otherCharacter in mapCharactersExceptSelf)
            {
                float totalScore = 0;

                foreach (var rule in rules)
                    totalScore += rule.CalculateTargetScore(decisionMaker, otherCharacter);
                
                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    primaryTarget = otherCharacter;
                }
            }
            return primaryTarget;
        }
        public List<CharacterBase> GetOppositeCharacter(CharacterBase character,
        List<CharacterBase> characterList)
        {
            List<CharacterBase> oppositeCharacter = new List<CharacterBase>();
            foreach (var otherCharacter in characterList)
            {
                if (otherCharacter.currentTeam != character.currentTeam)
                {
                    oppositeCharacter.Add(otherCharacter);
                }
            }
            return oppositeCharacter;
        }
        public List<CharacterBase> GetSameTeamCharacter(CharacterBase character,
        List<CharacterBase> characterList)
        {
            List<CharacterBase> sameTeamCharacter = new List<CharacterBase>();
            foreach (var otherCharacter in characterList)
            {
                if (otherCharacter.currentTeam == character.currentTeam)
                {
                    sameTeamCharacter.Add(otherCharacter);
                }
            }
            return sameTeamCharacter;
        }

        private List<GameNode> GetTargetAroundNodes(CharacterBase targetCharacter)
        {
            List<GameNode> primaryTargetAroundNodes = new List<GameNode>();

            int iteration = 1;
            int maxIteration = 10;

            while ((primaryTargetAroundNodes == null || primaryTargetAroundNodes.Count == 0) && iteration <= maxIteration)
            {
                primaryTargetAroundNodes = targetCharacter.GetCustomizedSizeMovableNodes(iteration, 0);
                Debug.Log($"Iteration {iteration}, nodes count: {primaryTargetAroundNodes.Count}");
                iteration++;
            }

            if (primaryTargetAroundNodes.Count != 0) return primaryTargetAroundNodes;
            return null;
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
                    UnitDetectable projectileDetect = skill.projectTilePrefab.GetComponent<UnitDetectable>();
                    
                    List<UnitDetectable> units = parabola.GetParabolaHitUnit
                        (projectileDetect, startNode.GetNodeVector() + new Vector3(0, shootOffsetHeight, 0),
                        targetNode.GetNodeVector(),
                        skill.initialElevationAngle);

                    if (units != null && units.Count > 0)
                    {
                        bool hitTeammate = false;
                        bool hitTarget = false;

                        foreach (UnitDetectable unit in units)
                        {
                            CharacterBase hitCharacter = unit.GetComponent<CharacterBase>();
                            if (hitCharacter == null) continue;

                            if (hitCharacter == targetNode.character)
                                hitTarget = true;

                            if (hitCharacter.currentTeam == decisionMaker.currentTeam)
                                hitTeammate = true;
                        }
                        if (hitTeammate) return false;

                        if (hitTarget) return true;
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

        private List<GameNode> GetSkillTargetableMovableNode(CharacterBase character, SkillData skill)
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
    }
}
