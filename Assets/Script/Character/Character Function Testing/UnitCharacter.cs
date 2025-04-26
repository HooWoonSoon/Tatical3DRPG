using System.Collections.Generic;
using UnityEngine;

public class UnitCharacter : MonoBehaviour
{
    [Header("Character Information")]
    public string characterName;
    public GameObject imageObject;
    public int ID;
    public int index { get; set; }


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
}