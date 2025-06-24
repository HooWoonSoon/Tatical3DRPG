using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        
    }

    public void RegisterScout(CharacterScouting scout)
    {
        scout.OnBattleTriggered += HandleBattleTriggered;
    }

    private void HandleBattleTriggered(List<Character> joinedBattleUnit)
    {
        TeamRetargetGridPlace.instance.EnterBattlePathFinding(joinedBattleUnit);
        //CTTimeline.instance.ReceiveBattleJoinedUnit(joinedBattleUnit);
        //for (int i = 0; i < joinedBattleUnit.Count; i++)
        //{
        //    joinedBattleUnit[i].isBattle = true;
        //}
    }
}

