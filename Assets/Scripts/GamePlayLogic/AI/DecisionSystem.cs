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
        public World world;
        public PathFinding pathFinding;
        public CharacterBase decisionMaker;

        private List<IScoreRule> targetRules = new List<IScoreRule>();
        private List<IScoreRule> skillRules = new List<IScoreRule>();
        private List<IScoreRule> moveToTargetRules = new List<IScoreRule>();
        private List<IScoreRule> moveRules = new List<IScoreRule>();
        private List<IScoreRule> orientationRule = new List<IScoreRule>();

        private SkillData skill;
        private GameNode moveNode;
        private GameNode skillTargetNode;
        private Orientation orientation;

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

            List<IScoreRule> targetSubRules = new List<IScoreRule>()
            {
            };
            targetRules.Add(new TargetRule(this, targetSubRules, 20, false));

            //  Move Rule
            List<IScoreRule> moveTargetSubRules = new List<IScoreRule>()
            {
                new TargetRule(this, null, 5, true)
            };
            moveToTargetRules.Add(new MoveTargetRule(this, moveTargetSubRules, 25, true));

            List<IScoreRule> moveSubRules = new List<IScoreRule>()
            {
                new MoveHarmRule(this, null, 15, false),
                new MoveTreatRule(this, null, 15, false)
            };
            moveRules.Add(new RiskMoveRule(this, moveSubRules, 25, false));

            //  SkillRule
            List<IScoreRule> harmSubRules = new List<IScoreRule>() 
            { 
                new FatalHitRule(this, null, 25, false)
            };
            skillRules.Add(new MoveHarmRule(this, harmSubRules, 330, false));

            List<IScoreRule> treatSubRules = new List<IScoreRule>()
            {
            };
            skillRules.Add(new MoveTreatRule(this, treatSubRules, 330, false));

            orientationRule.Add(new DefenseBackRule(this, null, 10, false));
        }

        public void MakeDecision(bool allowMove = true, bool allowSkill = true)
        {
            float startTime = Time.realtimeSinceStartup;

            EvaluateSkill(allowSkill, out float skillBestScore, 
                out SkillData originSkill, out GameNode originSkillTargetNode, 
                out string sourceSkill);

            EvaluateMoveAndSkill(allowMove, allowSkill, out float moveAndSkillBestScore,
                out SkillData moveSkill, out GameNode moveSkillMoveNode, 
                out GameNode moveSkillTargetNode, out string sourceMoveAndSkill);

            EvaluateMove(allowMove, allowSkill, out float moveBestScore, 
                out GameNode moveOnlyNode, out string sourceMove);

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

                if (originSkill != null && originSkillTargetNode != null)
                {
                    executeLog =
                    $"Decision: Origin Cast Skill, " +
                    $"Execute Option: {sourceSkill}, " +
                    $"Skill: {skill.skillName} at {skillTargetNode.GetNodeVectorInt()}";
                }
            }
            else if (decision == Decision.MoveAndCastSkill)
            {
                skill = moveSkill;
                moveNode = moveSkillMoveNode;
                skillTargetNode = moveSkillTargetNode;

                executeLog =
                    $"Decision: Move And Cast Skill, " +
                    $"Execute Option: {sourceMoveAndSkill}, " +
                    $"Move: {moveNode.GetNodeVectorInt()}, " +
                    $"Skill: {skill.skillName} at {skillTargetNode.GetNodeVectorInt()}";
            }
            else if (decision == Decision.Move)
            {
                skill = null;
                moveNode = moveOnlyNode;
                skillTargetNode = null;

                if (moveNode != null)
                {
                    executeLog =
                        $"Decision: Move, " +
                        $"Execute Option: {sourceMove}, " +
                        $"Move: {moveNode.GetNodeVectorInt()}";
                }
            }

            Orientation orientation = decisionMaker.selfOrientation;
            if (moveNode == null)
                EvaluateOrientation(decisionMaker.currentNode, ref orientation);
            else
                EvaluateOrientation(moveNode, ref orientation);
            this.orientation = orientation;

            if (debugMode)
                Debug.Log(
                    $"{decisionMaker.data.characterName}, " +
                    $"{executeLog}");

            float endTime = Time.realtimeSinceStartup;

            Debug.Log($"<color=green>[Make Decision]</color> " +
                $" completed in {endTime - startTime:F4} seconds");
        }

        #region Evaluation Methods
        private void EvaluateSkill(bool allowSkill, out float skillBestScore, 
            out SkillData originSkill, out GameNode originSkillTargetNode,
            out string source)
        {
            skillBestScore = float.MinValue;
            originSkill = null;
            originSkillTargetNode = null;
            source = null;

            if (allowSkill)
            {
                EvaluateOriginSkillOption(
                    ref skillBestScore,
                    ref originSkill,
                    ref originSkillTargetNode);
                source = "Evaluate Origin Skill Option";
            }
        }
        private void EvaluateMoveAndSkill(bool allowMove, bool allowSkill, 
            out float moveAndSkillBestScore, out SkillData moveSkill, 
            out GameNode moveSkillMoveNode, out GameNode moveSkillTargetNode,
            out string source)
        {
            moveAndSkillBestScore = float.MinValue;
            moveSkill = null;
            moveSkillMoveNode = null;
            moveSkillTargetNode = null;
            source = null;

            if (allowMove && allowSkill)
            {
                EvaluateMoveAndSkillOption(
                    ref moveAndSkillBestScore,
                    ref moveSkill,
                    ref moveSkillMoveNode,
                    ref moveSkillTargetNode);
                source = "Evaluate Move And Skill Option";
            }
        }
        private void EvaluateMove(bool allowMove, bool allowSkill, 
            out float moveBestScore, out GameNode moveOnlyNode, 
            out string soure)
        {
            moveBestScore = float.MinValue;
            moveOnlyNode = null;
            soure = null;
            orientation = decisionMaker.selfOrientation;

            if (allowMove && allowSkill)
            {
                EvaluateMoveTargetOption(
                    ref moveBestScore,
                    ref moveOnlyNode);
                soure = "Evaluate Move Target Option";
            }
            else if (allowMove && !allowSkill)
            {
                EvaluateMoveOption(
                    ref moveBestScore,
                    ref moveOnlyNode);
                soure = "Evaluate Move Option";
            }
        }
        private void EvaluateOrientation(GameNode originNode, ref Orientation bestOrientation)
        {
            float bestOrientationScore = int.MinValue;
            Orientation[] orientations = 
            {
                Orientation.right,
                Orientation.left,
                Orientation.back,
                Orientation.forward
            };

            foreach (var orientation in orientations)
            {
                float score = 0;
                foreach (var rule in orientationRule)
                {
                    score += rule.CalculateOrientationScore(decisionMaker, originNode, orientation);
                }

                if (score > bestOrientationScore)
                {
                    bestOrientation = orientation;
                    bestOrientationScore = score;
                }
            }
        }
        #endregion

        #region Evaluate Option Methods
        private void EvaluateOriginSkillOption(ref float bestScore,
            ref SkillData bestSkill, ref GameNode bestSkillTargetNode)
        {
            float startTime = Time.realtimeSinceStartup;

            GameNode originNode = decisionMaker.currentNode;

            bestSkill = null;
            bestSkillTargetNode = null;

            int maxHealthAmongOpposite = GetOppositeHighestHealth(decisionMaker);

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

                        foreach (var rule in skillRules)
                        {
                            totalScore += rule.CalculateMoveSkillScore(decisionMaker, skill, originNode, skillTargetNode, maxHealthAmongOpposite);
                        }


                        if (totalScore > bestScore)
                        {
                            bestScore = totalScore;
                            bestSkill = skill;
                            bestSkillTargetNode = skillTargetNode;
                        }
                    }
                }
            }

            float endTime = Time.realtimeSinceStartup;

            Debug.Log($"<color=green>[Evaluate Origin Skill Option]</color> " +
                $" completed in {endTime - startTime:F4} seconds");
        }
        private void EvaluateMoveAndSkillOption(ref float bestScore, 
            ref SkillData bestSkill, ref GameNode bestMoveNode, 
            ref GameNode bestSkillTargetNode)
        {
            float startTime = Time.realtimeSinceStartup;

            bestSkill = null;
            bestMoveNode = null;
            bestSkillTargetNode = null;

            int maxHealthAmongOpposite = GetOppositeHighestHealth(decisionMaker);

            foreach (SkillData skill in decisionMaker.skillDatas)
            {
                if (skill.isTargetTypeSkill)
                {
                    List<GameNode> skillTargetableMovableNode = GetSkillTargetableMovableNode(decisionMaker, skill);

                    if (debugMode)
                    {
                        List<GameNode> movableNodes = decisionMaker.GetMovableNodes();
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

                            foreach (var rule in skillRules)
                            {
                                totalScore += rule.CalculateMoveSkillScore(decisionMaker, skill, moveNode, skillTargetNode, maxHealthAmongOpposite);
                            }

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

            float endTime = Time.realtimeSinceStartup;

            Debug.Log($"<color=green>[Evaluate Move And Skill Option]</color> " +
                $" completed in {endTime - startTime:F4} seconds");
        }
        private void EvaluateMoveTargetOption(ref float bestScore, 
            ref GameNode bestNode)
        {
            float startTime = Time.realtimeSinceStartup;

            List<CharacterBase> mapCharactersExceptSelf = decisionMaker.GetMapCharacterExceptSelf();
            List<CharacterBase> opposites = GetOppositeCharacter(decisionMaker, mapCharactersExceptSelf);
            List<CharacterBase> teammates = GetSameTeamCharacter(decisionMaker, mapCharactersExceptSelf);

            CharacterSkillInfluenceNodes characterSkillInfluenceNodes = 
                new CharacterSkillInfluenceNodes(this, opposites, teammates);

            foreach (CharacterBase targetCharacter in mapCharactersExceptSelf)
            {
                List<GameNode> targetAroundNodes = GetTargetAroundNodes(targetCharacter);

                List<GameNode> movableNodes = decisionMaker.GetMovableNodes();
                foreach (var moveNode in movableNodes)
                {
                    float totalScore = 0;

                    foreach (var rule in moveToTargetRules)
                        totalScore += rule.CalculateMoveToTargetScore(decisionMaker, 
                            targetCharacter, targetAroundNodes, moveNode, teammates, opposites, characterSkillInfluenceNodes);

                    if (totalScore > bestScore)
                    {
                        bestScore = totalScore;
                        bestNode = moveNode;
                    }
                }
            }

            float endTime = Time.realtimeSinceStartup;

            Debug.Log($"<color=green>[Evaluate Move Target Option]</color> " +
                $" completed in {endTime - startTime:F4} seconds");
        }
        private void EvaluateMoveOption(ref float bestScore, 
            ref GameNode bestMoveNode)
        {
            float startTime = Time.realtimeSinceStartup;
            Debug.Log("Execute Evaluate Move Option");
            List<GameNode> movableNodes = decisionMaker.GetMovableNodes();

            List<CharacterBase> mapCharactersExceptSelf = 
                decisionMaker.GetMapCharacterExceptSelf();
            List<CharacterBase> opposites = 
                GetOppositeCharacter(decisionMaker, mapCharactersExceptSelf);
            List<CharacterBase> teammates =
                GetSameTeamCharacter(decisionMaker, mapCharactersExceptSelf);

            CharacterSkillInfluenceNodes characterSkillInfluenceNodes = 
                new CharacterSkillInfluenceNodes(this, opposites, teammates);

            float bestRiskMoveScore = 0;
            GameNode bestRiskMoveNode = null;

            foreach (var moveNode in movableNodes)
            {
                float riskMoveScore = 0;

                foreach (var rule in moveRules)
                {
                    riskMoveScore += rule.CalculateRiskMoveScore(decisionMaker,
                        characterSkillInfluenceNodes, moveNode);
                }
                if (riskMoveScore > bestScore)
                {
                    bestRiskMoveScore = riskMoveScore;
                    bestRiskMoveNode = moveNode;
                }
            }

            int highestBlocked = 0;
            GameNode bestBlockNode = null;
            foreach (var opposite in opposites)
            {
                int oppositeMovableNodeCount = opposite.GetMovableNodes().Count;
                foreach (var moveNode in movableNodes)
                {
                    int blockCount = CalculateBlockScore(decisionMaker, opposite, oppositeMovableNodeCount, moveNode);
                    Debug.Log($"Block Count: {blockCount} in {moveNode.GetNodeVectorInt()}");

                    if (blockCount > highestBlocked)
                    {
                        highestBlocked = blockCount;
                        bestBlockNode = moveNode;
                    }
                }
            }

            float selfFrontPosIndex = CalculateFrontPosIndex(decisionMaker, opposites);

            if (selfFrontPosIndex > 0 && bestBlockNode != null)
            {
                bestScore = 25;
                bestMoveNode = bestBlockNode;
            }

            if (selfFrontPosIndex < 0)
            {
                bestScore = bestRiskMoveScore;
                bestMoveNode = bestRiskMoveNode;
            }

            float endTime = Time.realtimeSinceStartup;

            Debug.Log($"<color=green>[Evaluate Move Option]</color> " +
                $" completed in {endTime - startTime:F4} seconds");
        }
        #endregion
        public int CalculateBlockScore(CharacterBase character, CharacterBase opposite,
            int oppositeMovableNodesCount, GameNode moveNode)
        {
            GameNode currentNode = character.currentNode;
            currentNode.character = null;
            moveNode.character = character;

            int currentOppositeMovableCount = opposite.GetMovableNodes().Count;

            currentNode.character = character;
            moveNode.character = null;

            return oppositeMovableNodesCount - currentOppositeMovableCount;
        }

        #region Front Score
        public CharacterBase GetMostHighFrontScoreCharacter(List<CharacterBase> characters, 
            List<CharacterBase> opposites)
        {
            float bestScore = float.MinValue;
            CharacterBase bestCharacter = null;
            foreach (var character in characters)
            {
                float score = CalculateFrontPosIndex(character, opposites);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCharacter = character;
                }
            }
            return bestCharacter;
        }

        /// <summary>
        /// Calculate the index of character are able to stand at the front line. 
        /// The index is based on the character's current health, damage absorption capacity, 
        /// and ranged combat capability.
        /// Sample position like tank and worrior
        /// </summary>
        /// <param name="character">The character required to evaluate</param>
        /// <param name="opposites">List of characters in the opposite team</param>
        /// <returns></returns>
        public float CalculateFrontPosIndex(CharacterBase character, 
            List<CharacterBase> opposites)
        {
            float score = 0f;

            int maxHealth = character.data.health;
            int currentHealth = character.currentHealth;
            float healthFactor = (float)currentHealth / maxHealth;
            float healthModifier = Mathf.Lerp(0.5f, 1, healthFactor);

            //  At least One Range
            float rangedAbility = CalculateRangedAbility(character);
            int abosoluteRangerIndex = 10;
            float tR = rangedAbility / abosoluteRangerIndex;
            float rangedModifier = Mathf.Lerp(0, 8, tR);

            //  At least One-time
            int takeDamageAbility = CalculateSurvivAbility(character, opposites);
            float absoluteSurvivalIndex = 8;
            float tS = takeDamageAbility / absoluteSurvivalIndex;
            float takeDamageModifier = Mathf.Lerp(0, 10, tS);

            score = healthModifier * takeDamageAbility - rangedModifier;

            Debug.Log(
                    $"<color=purple>[FrontPosIndex]</color> " +
                    $"{character.data.characterName}, " +
                    $"plus Score bonus: {score}");

            return score;
        }

        private float CalculateRangedAbility(CharacterBase character)
        {
            float rangeScore = 0;
            List<SkillData> skillData = character.skillDatas;
            int availableSkillCount = 0;
            foreach (var skill in skillData)
            {
                int mpCost = skill.MPAmount;
                float currentMetal = character.currentMental;
                if (mpCost > currentMetal) continue;
                availableSkillCount++;

                int range = skill.skillRange;
                int occulsiveRange = skill.occlusionRange;

                rangeScore += (range + occulsiveRange);
            }

            if (availableSkillCount == 0) return 1;
            if (rangeScore == 0) return 1;

            float skillScore = rangeScore / availableSkillCount;
            return skillScore;
        }
        /// <summary>
        /// Calculate the average damage of opposites team character available skills,
        /// then estimates the number of attacks the current character can withstand 
        /// before exhausting their health based on this average damage.
        /// </summary>
        /// <param name="character">The character required to evaluate</param>
        /// <param name="opposites">List of characters in the opposite team</param>
        /// <returns></returns>
        private int CalculateSurvivAbility(CharacterBase character,
            List<CharacterBase> opposites)
        {
            float combinedDamage = 0;
            foreach (CharacterBase opposite in opposites)
            {
                List<SkillData> skills = opposite.GetAvaliableSkills();

                var damageSkills = skills.Where(s => s.damageAmount > 0).ToList();

                if (damageSkills.Count > 0)
                {
                    float totalDamage = 0;
                    foreach (SkillData skill in damageSkills)
                    {
                        totalDamage += skill.damageAmount;
                    }
                    float averageDamage = totalDamage / damageSkills.Count;
                    combinedDamage += averageDamage;
                }
            }

            float combinedAverangeDamage = combinedDamage / opposites.Count;

            if (combinedAverangeDamage <= 0)
                return 999;

            int takeDamageTime = 0;
            float startHealth = character.data.health;
            while (startHealth > 0)
            {
                startHealth -= combinedAverangeDamage;
                takeDamageTime++;
            }

            return takeDamageTime;
        }
        #endregion

        public class CharacterSkillInfluenceNodes
        {
            public Dictionary<CharacterBase, Dictionary<SkillData, List<GameNode>>> oppositeInfluence;
            public Dictionary<CharacterBase, Dictionary<SkillData, List<GameNode>>> teammateInfluence;

            public CharacterSkillInfluenceNodes(DecisionSystem decisionSystem, List<CharacterBase> opposites, 
                List<CharacterBase> teammates, bool debugMode = false)
            {
                oppositeInfluence = new Dictionary<CharacterBase, Dictionary<SkillData, List<GameNode>>>();
                teammateInfluence = new Dictionary<CharacterBase, Dictionary<SkillData, List<GameNode>>>();

                foreach (var opposite in opposites)
                {
                    Dictionary<SkillData, List<GameNode>> skillCanAttackSet =
                    decisionSystem.GetOppositeSkillInfluence(opposite);

                    if (debugMode)
                    {
                        foreach (var kvp in skillCanAttackSet)
                        {
                            string skillName = kvp.Key.skillName;
                            string nodeLog = string.Join(", ", kvp.Value.ConvertAll(n => n.GetNodeVectorInt()));

                            Debug.Log(
                                $"Opposite: {opposite.data.characterName}, " +
                                $"Skill: {skillName}, " +
                                $"Influence Nodes: {nodeLog}");
                        }
                    }
                    oppositeInfluence[opposite] = skillCanAttackSet;
                }

                foreach (var teammate in teammates)
                {
                    Dictionary<SkillData, List<GameNode>> skillCanSupportSet =
                    decisionSystem.GetTeammateSkillInfluence(teammate);

                    if (debugMode)
                    {
                        foreach (var kvp in skillCanSupportSet)
                        {
                            string skillName = kvp.Key.skillName;
                            string nodeLog = string.Join(", ", kvp.Value.ConvertAll(n => n.GetNodeVectorInt()));

                            Debug.Log(
                                $"Teammate: {teammate.data.characterName}, " +
                                $"Skill: {skillName}, " +
                                $"Influence Nodes: {nodeLog}");
                        }
                    }
                    teammateInfluence[teammate] = skillCanSupportSet;
                }
            }
        }
        private Dictionary<SkillData, List<GameNode>> GetOppositeSkillInfluence
            (CharacterBase opposite)
        {
            Dictionary<SkillData, List<GameNode>> skillCanAttackSet = new Dictionary<SkillData, List<GameNode>>();

            int currentMental = opposite.currentMental;

            foreach (var skill in opposite.skillDatas)
            {
                if (skill.MPAmount > currentMental) continue;
                if (skill.skillTargetType != SkillTargetType.Opposite) continue;

                List<GameNode> oppositeMovableNodes = opposite.GetMovableNodes();
                HashSet<GameNode> canAttackNodes = new HashSet<GameNode>();

                foreach (var fromNode in oppositeMovableNodes)
                {
                    List<GameNode> skillScope = opposite.GetSkillRangeFromNode(skill, fromNode);

                    foreach (var scopeNode in skillScope)
                    {
                        canAttackNodes.Add(scopeNode);
                    }
                }
                skillCanAttackSet[skill] = canAttackNodes.ToList();
            }
            return skillCanAttackSet;
        }
        private Dictionary<SkillData, List<GameNode>> GetTeammateSkillInfluence
            (CharacterBase teammate)
        {
            Dictionary<SkillData, List<GameNode>> skillCanSupportSet = new Dictionary<SkillData, List<GameNode>>();

            int currentMental = teammate.currentMental;

            foreach (var skill in teammate.skillDatas)
            {
                if (currentMental > skill.MPAmount) continue;
                if (skill.skillTargetType == SkillTargetType.Opposite) continue;

                List<GameNode> oppositeMovableNodes = teammate.GetMovableNodes();
                HashSet<GameNode> canAttackNodes = new HashSet<GameNode>();

                foreach (var fromNode in oppositeMovableNodes)
                {
                    List<GameNode> skillScope = teammate.GetSkillRangeFromNode(skill, fromNode);

                    foreach (var scopeNode in skillScope)
                    {
                        canAttackNodes.Add(scopeNode);
                    }
                }
                skillCanSupportSet[skill] = canAttackNodes.ToList();
            }
            return skillCanSupportSet;
        }
        
        private int GetOppositeHighestHealth(CharacterBase character)
        {
            List<CharacterBase> mapCharacterEcexptSelf = character.GetMapCharacterExceptSelf();
            List<CharacterBase> opposites = GetOppositeCharacter(character, mapCharacterEcexptSelf);
            int highestHealth = 0;
            foreach (var opposite in opposites)
            {
                int health = opposite.data.health;
                if (health > highestHealth)
                {
                    highestHealth = health;
                }
            }
            return highestHealth;
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
                if (debugMode)
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
            List<GameNode> movableNodes = character.GetMovableNodes();
            HashSet<GameNode> result = new HashSet<GameNode>();

            if (!skill.isTargetTypeSkill) return movableNodes;

            GameNode originNode = character.currentNode;

            if (movableNodes == null || movableNodes.Count == 0) return null;
            
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
        public void GetResult(out SkillData skill, out GameNode moveToNode, 
            out GameNode skillTargetNode, out Orientation orientation)
        {
            skill = this.skill;
            moveToNode = this.moveNode;
            skillTargetNode = this.skillTargetNode;
            orientation = this.orientation;
        }
        #endregion
    }
}
