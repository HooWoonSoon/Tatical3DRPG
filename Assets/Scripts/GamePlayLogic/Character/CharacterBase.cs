using Assets.Script.BattleVisualTilemap;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class CharacterBase : Entity
{
    public enum Orientation
    {
        right, left, forward, back
    }

    public float moveSpeed = 5f;

    public TeamDeployment currentTeam;
    public UnitDetectable detectable;

    [Header("Character Information")]
    public CharacterData data;
    public int currenthealth;

    public PathRoute pathRoute;
    public Orientation orientation = Orientation.right;

    protected override void Start()
    {
        base.Start();
        detectable = GetComponent<UnitDetectable>();

        currenthealth = data.mentalPoint;
    }
    public void UpdateOrientation(Vector3 direction)
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
    public Vector3Int GetCharacterPosition()
    {
        return Utils.RoundXZFloorYInt(transform.position);
    }
    public void FacingDirection(Vector3 direction)
    {
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
    public void PathfindingMoveToTarget()
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
            
            pathRoute.character.transform.position = Vector3.MoveTowards(currentPos, nextPathPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(pathRoute.character.transform.position, nextPathPosition) <= 0.1f)
            {
                pathRoute.character.transform.position = nextPathPosition;
                pathRoute.pathIndex++;
                if (pathRoute.pathIndex >= pathRoute.pathRouteList.Count)
                {
                    Debug.Log($"Reached target {pathRoute.targetPosition}");
                    pathRoute.pathIndex = -1;
                    pathRoute = null;
                }
            }
        }
    }
    public bool IsYourTurn(CharacterBase character)
    {
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

    public List<GameNode> GetMovableGameNode()
    {
        int movableRange = data.movableRange;
        return pathFinding.GetCostDijkstraCoverangeNodes(Utils.RoundXZFloorYInt(transform.position), movableRange, 1, 1);
    }

    public void ResetVisualTilemap()
    {
        TilemapVisual.instance.InitializeValidPosition(GameNode.TilemapSprite.None);
    }
    public void ShowMovableTilemap()
    {
        int selfRange = data.movableRange;
        List<Vector3Int> reachableRange = pathFinding.GetCostDijkstraCoverangePos(Utils.RoundXZFloorYInt(transform.position), selfRange, 1, 1);
        foreach (Vector3Int position in reachableRange)
        {
            TilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
        }
    }
    public void ShowMovableTilemap(List<Vector3Int> coverage)
    {
        foreach (Vector3Int position in coverage)
        {
            TilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
        }
    }

    public void ShowDangerCoverageTileFromNode()
    {
        int selfRange = data.movableRange;
        Vector3Int selfPos = Utils.RoundXZFloorYInt(transform.position);
        List<GameNode> selfRangeExtend = pathFinding.GetCalculateDijkstraCost(selfPos, 1, 1);
        List<GameNode> selfMovableNode = pathFinding.GetCostDijkstraCoverangeNodes(selfPos, selfRange, 1, 1);
        HashSet<GameNode> selfMovableNodeSet = new HashSet<GameNode>(selfMovableNode);
        foreach (GameNode node in selfMovableNodeSet)
        {
            Vector3Int nodePos = node.GetVectorInt();
            TilemapVisual.instance.SetTilemapSprite(nodePos, GameNode.TilemapSprite.Blue);
        }

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
                        Vector3Int rangeNodePos = rangeNode.GetVectorInt();
                        TilemapVisual.instance.SetTilemapSprite(rangeNodePos, GameNode.TilemapSprite.Red);
                    }
                }
            }
        }
    }

    public void ShowDangerCoverageTile()
    {
        List<CharacterBase> characters = GetOppositeCharacter();
        int selfRange = data.movableRange;
        if (characters.Contains(this))
        {
            characters.Remove(this);
        }

        HashSet<Vector3Int> coverage = new HashSet<Vector3Int>();
        List<Vector3Int> reachableRange = pathFinding.GetCostDijkstraCoverangePos(Utils.RoundXZFloorYInt(transform.position), selfRange, 1, 1);
        foreach (Vector3Int position in reachableRange)
        {
            TilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
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
                    TilemapVisual.instance.SetTilemapSprite(pos, GameNode.TilemapSprite.Red);
                }
            }
        }
    }
    public void ShowMultipleCoverageTilemap(int selfRange, List<Vector3Int> coverage)
    {
        List<Vector3Int> reachableRange = pathFinding.GetCostDijkstraCoverangePos(Utils.RoundXZFloorYInt(transform.position), selfRange, 1, 1);
        foreach (Vector3Int position in reachableRange)
        {
            TilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
        }
        foreach (Vector3Int position in coverage)
        {
            TilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Red);
        }
    }
    public abstract void EnterBattle();
}