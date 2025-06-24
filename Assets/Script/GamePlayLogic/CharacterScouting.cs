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

        BattleManager.instance.RegisterScout(this);
    }

    private void Update()
    {
        List<Character> detectedCharacters = GetDetectedCharacter();

        if (detectedCharacters.Count > 0)
        {
            GetInfluenceUnits(detectedCharacters, out List<Character> joinedBattleUnit);
            OnBattleTriggered?.Invoke(joinedBattleUnit);
        }
    }

    private List<Character> GetDetectedCharacter()
    {
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
        return detectedCharacters;
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

