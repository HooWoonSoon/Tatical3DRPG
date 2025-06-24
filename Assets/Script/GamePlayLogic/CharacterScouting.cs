using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(UnitDetectable))]
public class CharacterScouting : MonoBehaviour
{
    private Character unitCharacter;
    private UnitDetectable selfDetectable;

    private Vector3 lastPosition;
    private float eslapseTime = 0;

    public event Action<List<Character>> OnBattleTriggered;

    public void Start()
    {
        selfDetectable = GetComponent<UnitDetectable>();
        unitCharacter = GetComponent<Character>();
    }

    private void Update()
    {
        if (unitCharacter.isBattle) { return; }
        UnitDetectable[] unitDetectable = selfDetectable.OverlapMahhatassRange(5);

        List<Character> detectedCharacters = new List<Character>();

        foreach (UnitDetectable hit in unitDetectable)
        {
            Character hitUnitCharacter = hit.GetComponent<Character>();
            if (hitUnitCharacter != null)
            {
                Debug.Log("true, Inside Mahhatass Range");
                detectedCharacters.Add(hitUnitCharacter);
            }
        }

        if (detectedCharacters.Count > 0)
        {
            GetInfluenceUnits(detectedCharacters, out List<Character> joinedBattleUnit);
            OnBattleTriggered?.Invoke(joinedBattleUnit);
            //TeamRetargetGridPlace.instance.EnterBattlePathFinding(joinedBattleUnit);
            //CTTimeline.instance.ReceiveBattleJoinedUnit(joinedBattleUnit);
            //for (int i = 0; i < joinedBattleUnit.Count; i++)
            //{
            //    joinedBattleUnit[i].isBattle = true;
            //}
        }
    }

    private void GetInfluenceUnits(List<Character> detectedCharacters, out List<Character> joinedBattleUnit)
    {
        joinedBattleUnit = new List<Character>();

        foreach (var detected in detectedCharacters)
        {
            foreach (var member in detected.currentTeam.teamCharacter)
            {
                if (!joinedBattleUnit.Contains(member))
                    joinedBattleUnit.Add(member);
            }
        }

        foreach (var member in unitCharacter.currentTeam.teamCharacter)
        {
            if (!joinedBattleUnit.Contains(member))
                joinedBattleUnit.Add(member);
        }
    }
}

