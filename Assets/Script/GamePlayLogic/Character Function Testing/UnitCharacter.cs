using System.Collections.Generic;
using UnityEngine;

public class UnitCharacter : MonoBehaviour
{
    public enum Type
    {
        Enemy, Allay, Neutral
    }

    public enum Orientation
    {
        right, left, forward, back
    }

    [Header("Character Information")]
    public string characterName;
    public GameObject imageObject;
    public int ID;
    public Type type;
    public int index { get; set; }

    //  Debug character properties
    [Header("Properties")]
    public int currenthealth;
    public int healthPoint;
    public int mentalPoint;
    public int magicAttackPoint;
    public int physicAttackPoint;
    public int speed;

    #region self path
    public readonly List<Vector3> positionHistory = new List<Vector3>();
    public int historyLimit { get; set; }

    private float recordInterval = 0.05f;
    private float recordTimer = 0f;
    #endregion

    #region state
    public bool isLeader;
    public bool isSelected;
    public bool isLink;
    public bool isBusy;
    
    public bool isMoving;
    public Vector3? targetPosition = null;
    public Orientation orientation = Orientation.right;
    #endregion

    public void UpdateHistory()
    {
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;

            if (positionHistory.Count >= historyLimit)
                positionHistory.RemoveAt(0);

            positionHistory.Add(this.transform.position);
        }
    }

    public void CleanAllHistory()
    {
        positionHistory.Clear();
    }

    public void FacingDirection(Vector3 direction)
    {
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
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