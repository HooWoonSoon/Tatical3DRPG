using Assets.Script.BattleVisualTilemap;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        return CTTimeline.instance.GetCurrentCharacter() == character;
    }
    public void ResetVisualTilemap()
    {
        TilemapVisual.instance.InitializeValidPosition(GameNode.TilemapSprite.None);
    }
    public void ShowVisualTilemapMahattasRange()
    {
        List<Vector3Int> movableRange = world.GetManhattas3DRange(Utils.RoundXZFloorYInt(transform.position), 4);
        foreach (Vector3Int position in movableRange)
        {
            TilemapVisual.instance.SetTilemapSprite(position, GameNode.TilemapSprite.Blue);
        }
    }
    public abstract void EnterBattle();
}