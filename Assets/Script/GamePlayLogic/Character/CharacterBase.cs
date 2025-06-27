using UnityEngine;

public class CharacterBase : Entity
{
    public enum Orientation
    {
        right, left, forward, back
    }

    public TeamDeployment currentTeam;
    public UnitDetectable detectable;

    [Header("Character Information")]
    public CharacterData data;
    public int currenthealth;

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
}