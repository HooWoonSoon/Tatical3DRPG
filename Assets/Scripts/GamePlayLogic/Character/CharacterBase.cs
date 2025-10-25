using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Orientation
{
    right, left, forward, back
}

public abstract class CharacterBase : Entity
{
    public GameObject characterModel;

    public float moveSpeed = 5f;

    public TeamDeployment currentTeam;
    public UnitDetectable detectable;

    [Header("Character Information")]
    public CharacterData data;
    public int currentHealth;
    public List<SkillData> skillData;

    public SkillData currentSkill { get; private set;}
    public GameNode currentSkillTargetNode { get; private set; }
    public PathRoute pathRoute { get; private set; }

    public Orientation orientation = Orientation.right;

    protected override void Start()
    {
        base.Start();
        detectable = GetComponent<UnitDetectable>();

        currentHealth = data.healthPoint;
    }

    public void SetOrientation(Orientation orientation)
    {
        this.orientation = orientation;
        switch (orientation)
        {
            case Orientation.left:
                transform.localScale = new Vector3(-1, 1, 1);
                break;
            case Orientation.right:
                transform.localScale = Vector3.one;
                break;
        }
    }

    public void SetOrientation(Vector3 direction)
    {
        direction = Vector3Int.RoundToInt(direction);

        if (direction == new Vector3(-1, 0, 0))
        {
            orientation = Orientation.left;
        }
        else if (direction == new Vector3(1, 0, 0))
        {
            orientation = Orientation.right;
        }
        else if (direction == new Vector3(0, 0, -1))
        {
            orientation = Orientation.back;
        }
        else if (direction == new Vector3(0, 0, 1))
        {
            orientation = Orientation.forward;
        }
    }
    public Vector3Int GetOrientationVector()
    {
        switch (orientation)
        {
            case Orientation.left:
                return new Vector3Int(-1, 0, 0);
            case Orientation.right:
                return new Vector3Int(1, 0, 0);
            case Orientation.back:
                return new Vector3Int(0, 0, -1);
            case Orientation.forward:
                return new Vector3Int(0, 0, 1);
            default:
                return Vector3Int.zero;
        }
    }
    public Vector3Int GetCharacterNodePosition()
    {
        return Utils.RoundXZFloorYInt(transform.position);
    }

    public GameNode GetCharacterOriginNode()
    {
        return world.GetNode(GetCharacterNodePosition());
    }

    public void FacingDirection(Vector3 direction)
    {
        if (direction.x > 0)
            transform.localScale = Vector3.one;
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
   
    public void SetSelfToNode(GameNode targetNode, float offsetY = 0.5f)
    {
        Vector3 targetPos = targetNode.GetVector();
        transform.position = targetPos + new Vector3(0, offsetY, 0);
        targetNode.SetUnitGridCharacter(this);
    }


    public void SetPathRoute(PathRoute pathRoute)
    {
        this.pathRoute = pathRoute;
        //pathRoute.DebugPathRoute();
    }

    public void SetPathRoute(GameNode targetNode)
    {
        if (targetNode == null) return;
        pathRoute = GetPathRoute(targetNode);
        //pathRoute.DebugPathRoute();
    }

    public PathRoute GetPathRoute(GameNode targetNode)
    {
        Vector3 selfPos = GetCharacterNodePosition();
        Vector3 targetPos = targetNode.GetVector();
        List<Vector3> pathVectorList = pathFinding.GetPathRoute(selfPos, targetPos, 1, 1).pathRouteList;
        return new PathRoute
        {
            character = this,
            targetPosition = targetNode.GetVectorInt(),
            pathRouteList = pathVectorList,
            pathIndex = 0
        };
    }

    public void PathToTarget()
    {
        if (pathRoute == null) return;

        if (pathRoute.pathIndex != -1)
        {
            Vector3 nextPathPosition = pathRoute.pathRouteList[pathRoute.pathIndex];
            Vector3 currentPos = pathRoute.character.transform.position;
            Vector3 direction = (nextPathPosition - currentPos).normalized;

            float heightDifferent = nextPathPosition.y - currentPos.y;
            if (Mathf.Abs(heightDifferent) > 0.1f)
            {
                Vector3 heightPos = new Vector3(currentPos.x, currentPos.y + heightDifferent, currentPos.z);
                pathRoute.character.transform.position = Vector3.MoveTowards(currentPos, heightPos, moveSpeed * 2 * Time.deltaTime);
                currentPos = pathRoute.character.transform.position;
            }

            FacingDirection(direction);
            SetOrientation(direction);
            
            pathRoute.character.transform.position = Vector3.MoveTowards(currentPos, nextPathPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(pathRoute.character.transform.position, nextPathPosition) <= 0.1f)
            {
                pathRoute.character.transform.position = nextPathPosition;
                pathRoute.pathIndex++;
                GridCharacter.instance.SetGridCharacter(Utils.RoundXZFloorYInt(transform.position), this);
                if (pathRoute.pathIndex >= pathRoute.pathRouteList.Count)
                {
                    //Debug.Log($"Reached target {pathRoute.targetPosition}");
                    pathRoute.pathIndex = -1;
                    pathRoute = null;
                }
            }
        }
    }

    public void SetSkill(SkillData skill)
    {
        if (skill == null) { return; }
        currentSkill = skill;
    }
    
    public void SetSkillAndTarget(SkillData skill, GameNode targetNode)
    {
        if (skill == null) { return; }
        currentSkill = skill;
        currentSkillTargetNode = targetNode;
    }

    public void SkillCalculate()
    {
        CharacterBase character = currentSkillTargetNode.GetUnitGridCharacter();
        if (currentSkill.skillType == SkillType.Acttack)
        {
            int damage = currentSkill.damageAmount;
            if (character != null)
            {
                character.currentHealth -= damage;
                //Debug.Log($"{this.gameObject.name} damage {character.gameObject.name} for {damage} points. Remaining health: {character.currenthealth}");
                BattleUIManager.instance.CreateCountText(character, damage);
            }
        }
        else if (currentSkill.skillType == SkillType.Heal)
        {
            int heal = currentSkill.healAmount;
            if (character != null)
            {
                character.currentHealth += heal;
                BattleUIManager.instance.CreateCountText(character, heal);
            }
        }
        currentSkill = null;
    }

    public bool IsYourTurn(CharacterBase character)
    {
        if (!BattleManager.instance.isBattleStarted) { return false; }
        if (character == CTTimeline.instance.GetCurrentCharacter()) return true;
        return false;
    }

    public List<CharacterBase> GetOppositeCharacter()
    {
        List<CharacterBase> oppositeCharacter = new List<CharacterBase>();
        List<TeamDeployment> battleTeam = BattleManager.instance.GetBattleTeam();
        foreach (var team in battleTeam)
        {
            foreach (var character in team.teamCharacter)
            {
                if (character.currentTeam != currentTeam)
                {
                    oppositeCharacter.Add(character);
                }
            }
        }
        return oppositeCharacter;
    }

    public List<Vector3Int> GetUnlimitedMovablePos(int size)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        Vector3Int selfPos = GetCharacterNodePosition();
        List<GameNode> coverage = pathFinding.GetCostDijkstraCoverangeNodes(selfPos, size, 1, 1);
        foreach (var node in coverage)
        {
            if (node.character == null || node.character == this)
            {
                result.Add(node.GetVectorInt());
            }
        }
        return result;
    }

    public List<GameNode> GetMovableNode()
    {
        List<GameNode> result = new List<GameNode>();
        int movableRange = data.movableRange;
        Vector3Int selfPos = GetCharacterNodePosition();
        List<GameNode> coverage = pathFinding.GetCostDijkstraCoverangeNodes(selfPos, movableRange, 1, 1);
        foreach (var node in coverage)
        {
            if (node.character == null || node.character == this)
            {
                result.Add(node);
            }
        }
        return result;
    }

    public List<GameNode> GetConflictNode()
    {
        int selfRange = data.movableRange;
        Vector3Int selfPos = GetCharacterNodePosition();
        List<GameNode> selfRangeExtend = pathFinding.GetCalculateDijkstraCost(selfPos, 1, 1);
        List<GameNode> selfMovableNode = pathFinding.GetCostDijkstraCoverangeNodes(selfPos, selfRange, 1, 1);
        HashSet<GameNode> selfMovableNodeSet = new HashSet<GameNode>(selfMovableNode);
        List<GameNode> conflictNode = new List<GameNode>();
        foreach (GameNode node in selfRangeExtend)
        {
            if (node.character == null) continue;
            CharacterBase character = node.character;
            TeamDeployment oppositeTeam = character.currentTeam;
            if (oppositeTeam != currentTeam)
            {
                int oppositeRange = character.data.movableRange;
                Vector3Int oppositePos = Utils.RoundXZFloorYInt(character.transform.position);
                List<GameNode> oppositeRangeNodes = pathFinding.GetCostDijkstraCoverangeNodes(oppositePos, oppositeRange, 1, 1);
                foreach (GameNode rangeNode in oppositeRangeNodes)
                {
                    if (selfMovableNodeSet.Contains(rangeNode))
                    {
                        conflictNode.Add(rangeNode);
                    }
                }
            }
        }
        return conflictNode;
    }

    public List<GameNode> GetSkillAttackMovableNode(SkillData skill)
    {
        List<GameNode> movableNodes = GetMovableNode();
        HashSet<GameNode> result = new HashSet<GameNode>();

        foreach (GameNode moveNode in movableNodes)
        {
            List<GameNode> influenceNodes = skill.GetInflueneNode(world, moveNode);
            foreach (GameNode node in influenceNodes)
            {
                if (node.character != null && node.character != this && 
                    node.character.currentTeam != currentTeam)
                {
                    result.Add(moveNode);
                    break;
                }
            }
        }
        return result.ToList();
    }

    public List<GameNode> GetSkillRangeFromNode(SkillData skill, GameNode gameNode)
    {
        return skill.GetInflueneNode(world, gameNode);
    }
    private List<GameNode> GetSkillRangeFromNode(SkillData skill)
    {
        return skill.GetInflueneNode(world, world.GetNode(GetCharacterNodePosition()));
    }

    public List<CharacterBase> GetSkillAttackableCharacter(SkillData skill, GameNode gameNode)
    {
        HashSet<CharacterBase> result = new HashSet<CharacterBase>();
        List<GameNode> influenceNodes = skill.GetInflueneNode(world, gameNode);

        foreach (GameNode node in influenceNodes)
        {
            if (node.character != null && node.character != this &&
                node.character.currentTeam != currentTeam)
            {
                result.Add(node.character);
            }
        }
        return result.ToList();
    }

    public void ResetVisualTilemap()
    {
        GridTilemapVisual.instance.SetAllTileSprite(GameNode.TilemapSprite.None);
    }

    public void ShowMovableTilemap()
    {
        ResetVisualTilemap();
        int selfRange = data.movableRange;
        List<GameNode> movableNode = GetMovableNode();
        foreach (GameNode node in movableNode)
        {
            Vector3Int position = node.GetVectorInt();
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
        }
    }

    public void ShowDangerAndMovableTileFromNode()
    {
        ResetVisualTilemap();
        List<GameNode> selfMovableNode = GetMovableNode();
        foreach (GameNode node in selfMovableNode)
        {
            Vector3Int nodePos = node.GetVectorInt();
            GridTilemapVisual.instance.SetTilemapSprite(nodePos, GameNode.TilemapSprite.Blue);
        }
        List<GameNode> dangerNode = GetConflictNode();
        foreach (GameNode rangeNode in dangerNode)
        {
            Vector3Int rangeNodePos = rangeNode.GetVectorInt();
            GridTilemapVisual.instance.SetTilemapSprite(rangeNodePos, GameNode.TilemapSprite.Purple);
        }
    }
    public void ShowDangerAndMovableTile()
    {
        ResetVisualTilemap();
        List<CharacterBase> characters = GetOppositeCharacter();
        int selfRange = data.movableRange;

        HashSet<Vector3Int> coverage = new HashSet<Vector3Int>();
        List<Vector3Int> reachableRange = pathFinding.GetCostDijkstraCoverangePos(Utils.RoundXZFloorYInt(transform.position), selfRange, 1, 1);
        foreach (Vector3Int position in reachableRange)
        {
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
        }
        for (int i = 0; i < characters.Count; i++)
        {
            int oppositeRange = characters[i].data.movableRange;
            List<Vector3Int> coverageRange = pathFinding.GetCostDijkstraCoverangePos(Utils.RoundXZFloorYInt(characters[i].transform.position), oppositeRange, 1, 1);
            foreach (var pos in coverageRange)
            {
                if (reachableRange.Contains(pos))
                {
                    coverage.Add(pos); 
                    GridTilemapVisual.instance.SetTilemapSprite(pos, GameNode.TilemapSprite.Purple);
                }
            }
        }
    }

    public void ShowMultipleCoverageTilemap(int selfRange, List<Vector3Int> coverage)
    {
        ResetVisualTilemap();
        List<Vector3Int> reachableRange = pathFinding.GetCostDijkstraCoverangePos(Utils.RoundXZFloorYInt(transform.position), selfRange, 1, 1);
        foreach (Vector3Int position in reachableRange)
        {
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
        }
        foreach (Vector3Int position in coverage)
        {
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Purple);
        }
    }

    public void ShowDangerMovableAndTargetTilemap(GameNode targetNode)
    {
        ResetVisualTilemap();
        List<GameNode> movableNode = GetMovableNode();
        if (movableNode.Contains(targetNode))
        {
            foreach (GameNode gameNode in movableNode)
            {
                Vector3Int position = gameNode.GetVectorInt();
                GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
            }
            List<GameNode> conflictNode = GetConflictNode();
            foreach (GameNode gameNode in conflictNode)
            {
                Vector3Int position = gameNode.GetVectorInt();
                GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Purple);
            }
            GridTilemapVisual.instance.SetTilemapSprite(targetNode.GetVectorInt(), GameNode.TilemapSprite.TinyBlue);
        }
    }

    public void ShowSkillTargetTilemap()
    {
        if (currentSkillTargetNode == null) { return; }
        ShowSkillTilemap();
        Vector3Int position = currentSkillTargetNode.GetVectorInt();
        GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Red);
    }
    public void ShowSkillTargetTilemap(GameNode originNode, GameNode targetNode)
    {
        ShowSkillTilemap(originNode);
        List<GameNode> skillRangeNodes = GetSkillRangeFromNode(currentSkill, originNode);
        if (skillRangeNodes.Contains(targetNode))
        {
            Vector3Int position = targetNode.GetVectorInt();
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Red);
        }
    }

    public GameNode GetSkillTargetShowTilemap(GameNode originNode, GameNode targetNode)
    {
        ShowSkillTilemap(originNode);
        List<GameNode> skillRangeNodes = GetSkillRangeFromNode(currentSkill, originNode);
        if (skillRangeNodes.Contains(targetNode))
        {
            Vector3Int position = targetNode.GetVectorInt();
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Red);
            return targetNode;
        }
        return null;
    }

    public void ShowSkillTilemap()
    {
        if (currentSkill == null) return;
        ResetVisualTilemap();
        List<GameNode> influenceNodes = GetSkillRangeFromNode(currentSkill);
        foreach (GameNode node in influenceNodes)
        {
            Vector3Int position = node.GetVectorInt();
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.TinyBlue);
        }
    }
    public void ShowSkillTilemap(GameNode gameNode)
    {
        if (currentSkill == null) return;
        ResetVisualTilemap();
        List<GameNode> influenceNodes = GetSkillRangeFromNode(currentSkill, gameNode);
        foreach (GameNode node in influenceNodes)
        {
            Vector3Int position = node.GetVectorInt();
            GridTilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.TinyBlue);
        }
    }

    public bool IsInMovableRange(GameNode gameNode)
    {
        HashSet<GameNode> movableNodes = new HashSet<GameNode>(GetMovableNode());
        if (movableNodes.Contains(gameNode))
        {
            return true;
        }
        return false;
    }

    public abstract void TeleportToNodeFree(GameNode targetNode);
    public abstract void TeleportToNodeDeployble(GameNode targetNode);
    public abstract void ReadyBattle();
}