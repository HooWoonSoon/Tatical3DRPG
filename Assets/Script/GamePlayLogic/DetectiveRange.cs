using System.Collections.Generic;
using System;
using UnityEngine;
public class DetectiveRange : MonoBehaviour
{
    public List<UnitDetectable> unitDetectables = new List<UnitDetectable>();
    public NonPlayerTeam nonPlayerTeam;
    private Vector3 lastPosition;
    private float eslapseTime = 0;

    private bool isBattle = false;

    public void Awake()
    {
        unitDetectables = new List<UnitDetectable>(FindObjectsOfType<UnitDetectable>());
    }

    private void Start()
    {
        nonPlayerTeam = GetComponent<NonPlayerTeam>();
        if (nonPlayerTeam == null)
        {
            Debug.LogError("NonPlayerTeam component not found on this GameObject.");
        }
    }

    private void Update()
    {
        if (isBattle) { return; }

        for (int i = 0; i < unitDetectables.Count; i++)
        {
            if (IsInsideMahhatassRange(5, Utils.RoundXZFloorYInt(transform.position), unitDetectables[i].GetPositionRoundXZIntY()))
            {
                Debug.Log("true, Inside Mahhatass Range");
                CTTimeline.instance.ReceiveBattleJoinedUnit(GetInfluenceUnits());
                isBattle = true;
            }
            else
            {
                Debug.Log("false, Outside Mahhatass Range");
            }
        }
    }

    private bool IsInsideMahhatassRange(int mahhatassRange, Vector3Int A, Vector3Int B)
    {
        int mahhatassDistance = Mathf.Abs(A.x - B.x) + Mathf.Abs(A.y - B.y) + Mathf.Abs(A.z - B.z);
        return mahhatassDistance <= mahhatassRange; 
    }

    //  Debug
    private List<UnitCharacter> GetInfluenceUnits()
    {
        List<UnitCharacter> unitCharacters = new List<UnitCharacter>();

        for (int i = 0; i < TeamFollowSystem.instance.teamFollowers.Count; i++)
        {
            unitCharacters.Add(TeamFollowSystem.instance.teamFollowers[i].unitCharacter);
        }
        for (int j = 0; j < nonPlayerTeam.unitCharacters.Count; j++)
        {
            unitCharacters.Add(nonPlayerTeam.unitCharacters[j]);
        }
        return unitCharacters;
    }

    //  Summary
    //      Debug
    //      Use to bake the Mahhatass Range with update frame 
    //      Maybe not use
    private bool IsPositionChange()
    {
        float updateFrame = 0.1f;

        eslapseTime += Time.deltaTime;
        
        if (eslapseTime < updateFrame) { return false; }

        if (transform.position != lastPosition)
        {
            lastPosition = transform.position;
            eslapseTime = 0;
            return true;
        }
        return false;
    }
}

