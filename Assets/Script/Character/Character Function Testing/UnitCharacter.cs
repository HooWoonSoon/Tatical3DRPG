using System.Collections.Generic;
using UnityEngine;

public class UnitCharacter : MonoBehaviour
{
    public string characterName;
    public GameObject imageObject;
    public int ID;
    public int index { get; set; }
    public bool isLink { get; set; }
    public bool isLeader;

    public readonly List<Vector3> positionHistory = new List<Vector3>();
    public int historyLimit = 15;

    private float recordInterval = 0.1f;
    private float recordTimer = 0f;

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

    public void FacingDirection(Vector3 direction)
    {
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
}