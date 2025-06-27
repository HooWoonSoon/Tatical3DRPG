using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(UnitDetectable))]
public class CharacterScouting : MonoBehaviour
{
    private CharacterBase unitCharacter;
    private UnitDetectable selfDetectable;

    private Vector3 lastPosition;
    private float eslapseTime = 0;

    public event Action<List<CharacterBase>> OnBattleTriggered;

    public void Start()
    {
        selfDetectable = GetComponent<UnitDetectable>();
        unitCharacter = GetComponent<CharacterBase>();
    }

    private void Update()
    {
        List<CharacterBase> detectedCharacters = GetDetectedCharacter();

        if (detectedCharacters.Count > 0)
        {
            GetInfluenceUnits(detectedCharacters, out List<CharacterBase> joinedBattleUnit);
            OnBattleTriggered?.Invoke(joinedBattleUnit);
        }
    }

    private List<CharacterBase> GetDetectedCharacter()
    {
        UnitDetectable[] unitDetectable = selfDetectable.OverlapMahhatassRange(5);
        List<CharacterBase> detectedCharacters = new List<CharacterBase>();

        foreach (UnitDetectable hit in unitDetectable)
        {
            CharacterBase hitUnitCharacter = hit.GetComponent<CharacterBase>();
            if (hitUnitCharacter != null)
            {
                Debug.Log("true, Inside Mahhatass Range");
                detectedCharacters.Add(hitUnitCharacter);
            }
        }
        return detectedCharacters;
    }

    private void GetInfluenceUnits(List<CharacterBase> detectedCharacters, out List<CharacterBase> joinedBattleUnit)
    {
        joinedBattleUnit = new List<CharacterBase>();

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

